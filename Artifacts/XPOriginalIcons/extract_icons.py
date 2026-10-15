import io
import struct
from pathlib import Path
from PIL import Image, ImageDraw

root = Path(__file__).parent
data = (root / 'shell32.dll').read_bytes()
pe = struct.unpack_from('<I', data, 0x3c)[0]
section_count = struct.unpack_from('<H', data, pe + 6)[0]
optional_size = struct.unpack_from('<H', data, pe + 20)[0]
optional = pe + 24
directory = optional + (96 if struct.unpack_from('<H', data, optional)[0] == 0x10b else 112)
resource_rva = struct.unpack_from('<I', data, directory + 16)[0]
sections = []
for i in range(section_count):
    offset = optional + optional_size + 40 * i
    virtual_size, rva, raw_size, raw = struct.unpack_from('<IIII', data, offset + 8)
    sections.append((rva, max(virtual_size, raw_size), raw))

def file_offset(rva):
    return next(raw + rva - start for start, size, raw in sections if start <= rva < start + size)

resource_base = file_offset(resource_rva)
resources = {}

def walk(offset, path):
    named, numbered = struct.unpack_from('<HH', data, resource_base + offset + 12)
    for i in range(named + numbered):
        key, target = struct.unpack_from('<II', data, resource_base + offset + 16 + i * 8)
        if key & 0x80000000:
            continue
        if target & 0x80000000:
            walk(target & 0x7fffffff, path + (key,))
        else:
            rva, size = struct.unpack_from('<II', data, resource_base + target)
            resources[path + (key,)] = data[file_offset(rva):file_offset(rva) + size]

walk(0, ())
output = root / 'extracted'
output.mkdir(exist_ok=True)
icons = []
for (kind, number, language), group in sorted(resources.items()):
    if kind != 14:
        continue
    count = struct.unpack_from('<H', group, 4)[0]
    payload_offset = 6 + count * 16
    entries, payloads = [], []
    for i in range(count):
        width, height, colors, reserved, planes, bits, size, icon_id = struct.unpack_from('<BBBBHHIH', group, 6 + 14 * i)
        payload = resources.get((3, icon_id, language))
        if payload is None:
            payload = next(v for k, v in resources.items() if k[:2] == (3, icon_id))
        entries.append(struct.pack('<BBBBHHII', width, height, colors, reserved, planes, bits, len(payload), payload_offset))
        payloads.append(payload)
        payload_offset += len(payload)
    ico = group[:6] + b''.join(entries) + b''.join(payloads)
    (output / f'{number}.ico').write_bytes(ico)
    image = Image.open(io.BytesIO(ico))
    sizes = image.ico.sizes()
    selected = (32, 32) if (32, 32) in sizes else max(sizes)
    png = image.ico.getimage(selected).convert('RGBA')
    png.save(output / f'{number}.png')
    icons.append((number, png))

columns, cell = 16, 56
sheet = Image.new('RGB', (columns * cell, ((len(icons) + columns - 1) // columns) * cell), '#eee9da')
draw = ImageDraw.Draw(sheet)
for i, (number, png) in enumerate(icons):
    x, y = i % columns * cell, i // columns * cell
    preview = png.copy()
    preview.thumbnail((32, 32))
    sheet.paste(preview, (x + 12, y + 3), preview)
    draw.text((x + 13, y + 38), str(number), fill='black')
sheet.save(root / 'contact-sheet.png')
print(f'Extracted {len(icons)} icon groups from XP shell32.dll.')
