-- Derive color-only variants from the existing brush animation.
-- Keep every source pixel except the three bristle colors unchanged.
local base = "D:/Github/Engine/"
local outDir = base .. "Assets/RYU/Sprite/Charcter/Paint/BrushColors/"
local workDir = base .. "Artifacts/Paint/BrushColors/"
local pc = app.pixelColor
local bristleColors = {
  pc.rgba(172,167,149,255), -- mid
  pc.rgba(119,125,116,255), -- shade
  pc.rgba(222,215,190,255)  -- light
}
local variants = {
  { name="Red", colors={{238,67,59},{164,52,51},{255,140,103}} },
  { name="Yellow", colors={{248,200,48},{194,144,35},{255,234,130}} },
  { name="Blue", colors={{59,148,224},{45,97,163},{140,211,247}} },
  { name="TipMask", colors={{205,205,205},{145,145,145},{255,255,255}} }
}
local native = Sprite(128,128,ColorMode.RGB)
local layers = {}
for n,variant in ipairs(variants) do
  layers[n] = n==1 and native.layers[1] or native:newLayer()
  layers[n].name = variant.name
  layers[n].isVisible = n==1
end
local preview = Image(1536,512,ColorMode.RGB)
preview:clear()
local durations = {0.08,0.08,0.09}
local report = {}
for frame=1,3 do
  if frame>1 then native:newFrame() end
  native.frames[frame].duration = durations[frame]
  local input = Image{fromFile=base.."Assets/RYU/Sprite/Charcter/Paint/Brush_"..string.format("%02d",frame)..".png"}
  assert(input.width==512 and input.height==512,"Unexpected source size")
  for n,variant in ipairs(variants) do
    local image = Image(512,512,ColorMode.RGB)
    image:clear()
    local count = 0
    for y=0,511 do for x=0,511 do
      local value = input:getPixel(x,y)
      local index = nil
      for i,original in ipairs(bristleColors) do if value==original then index=i;break end end
      if index then
        local c = variant.colors[index]
        image:drawPixel(x,y,pc.rgba(c[1],c[2],c[3],255))
        count = count+1
      elseif variant.name~="TipMask" then
        image:drawPixel(x,y,value)
      end
    end end
    assert(count>100,"Bristle colors were not found")
    local small = Image(128,128,ColorMode.RGB)
    for y=0,127 do for x=0,127 do small:drawPixel(x,y,image:getPixel(x*4,y*4)) end end
    local cel = layers[n]:cel(frame)
    if cel then cel.image=small;cel.position=Point(0,0) else native:newCel(layers[n],frame,small,Point(0,0)) end
    local fileName = "Brush_"..variant.name.."_"..string.format("%02d",frame)..".png"
    image:saveAs(outDir..fileName)
    if frame==1 and n<=3 then preview:drawImage(image,Point((n-1)*512,0)) end
    report[#report+1] = fileName..": bristle pixels="..count
  end
end
native.data = "Color-only brush variants. Original128px grid; export nearest4x to512. Red/yellow/blue match Paint palette. TipMask contains only bristles, for optional SpriteRenderer tint overlay. Frames80/80/90ms. Source geometry unchanged."
native:saveAs(workDir.."BrushColors.aseprite")
preview:saveAs(workDir.."BrushColors-preview.png")
local log = assert(io.open(workDir.."aseprite-export.txt","w"))
log:write("Aseprite "..tostring(app.version).."\n"..table.concat(report,"\n").."\n")
log:close()
native:close()
