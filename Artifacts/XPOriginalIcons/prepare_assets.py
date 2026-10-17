from pathlib import Path
from PIL import Image

root = Path(__file__).parent
destination = root.parents[1] / 'Assets/RYU/Sprite/XPTaskbar'
for name, source in [('monitor', '16'), ('folder', '4'), ('network', '18')]:
    image = Image.open(root / 'extracted' / f'{source}.ico')
    image.ico.getimage((32, 32)).convert('RGBA').save(destination / f'{name}.png')
for name, source in [('speaker', 'volume'), ('speaker-muted', 'mute')]:
    image = Image.open(root / f'{source}.ico')
    sizes = image.ico.sizes()
    size = (32, 32) if (32, 32) in sizes else max(sizes)
    image.ico.getimage(size).convert('RGBA').save(destination / f'{name}.png')
    print(name, sorted(sizes))
