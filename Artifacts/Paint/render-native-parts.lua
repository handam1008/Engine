local output = app.params.output or "D:/Github/Engine/Artifacts/Paint/Paint-Parts-v3"
local base = "D:/Github/Engine/Artifacts/Paint/"
local paletteData = dofile(base .. "palette-native-data.lua")
local brushData = dofile(base .. "brush-native-data.lua")
local sprite = Sprite(256,128,ColorMode.RGB)
sprite.gridBounds = Rectangle(0,0,2,2)
sprite.data = "Integer-pixel artwork authored with Lua in Aseprite. Two 128px cells; primary clusters2px. No AI image resampling, gradients, anti-aliasing or image filtering."
local function raster(image, item)
  local vertices = item.points
  local c=item.color
  local color=app.pixelColor.rgba(c[1],c[2],c[3],c[4] or 255)
  for y=0,63 do
    local scanY=y+0.5
    local intersections={}
    for i=1,#vertices do
      local a=vertices[i]
      local b=vertices[i % #vertices+1]
      if (a[2]<=scanY and b[2]>scanY) or (b[2]<=scanY and a[2]>scanY) then
        intersections[#intersections+1]=a[1]+(scanY-a[2])*(b[1]-a[1])/(b[2]-a[2])
      end
    end
    table.sort(intersections)
    for i=1,#intersections-1,2 do
      for x=math.max(0,math.ceil(intersections[i]-0.5)),math.min(63,math.floor(intersections[i+1]-0.5)) do image:drawPixel(x,y,color) end
    end
  end
end
local function layer(name,items,offsetX)
  local small=Image(64,64,ColorMode.RGB)
  small:clear()
  for _,item in ipairs(items) do raster(small,item) end
  local native=Image(128,128,ColorMode.RGB)
  native:clear()
  for y=0,127 do for x=0,127 do native:drawPixel(x,y,small:getPixel(math.floor(x/2),math.floor(y/2))) end end
  local target=sprite:newLayer()
  target.name=name
  sprite:newCel(target,1,native,Point(offsetX,0))
end
layer("Palette - wood and thumbhole",paletteData.polygons,0)
layer("Palette - red yellow blue",paletteData.paint,0)
layer("Brush",brushData.polygons,128)
-- The initial empty layer is harmless but omitted from the editable source.
sprite:deleteLayer(sprite.layers[1])
local colors={}
local seen={}
for _,cel in ipairs(sprite.cels) do
  for pixel in cel.image:pixels() do local value=pixel();if not seen[value] then seen[value]=true;colors[#colors+1]=value end end
end
local fixed=Palette(#colors)
for i,value in ipairs(colors) do fixed:setColor(i-1,value) end
sprite:setPalette(fixed)
sprite:saveAs(output .. ".aseprite")
-- Explicit integer nearest-neighbor copy; output cannot contain fractional pixels.
local flat=Image(sprite)
flat:saveAs(output .. "-logical.png")
local exported=Image(1024,512,ColorMode.RGB)
for y=0,511 do for x=0,1023 do exported:drawPixel(x,y,flat:getPixel(math.floor(x/4),math.floor(y/4))) end end
exported:saveAs(output .. ".png")
local report=io.open(output .. "-aseprite-run.txt","w")
report:write("Aseprite "..tostring(app.version).."\nNative256x128; percell128x128; integer clusters2x2; PNG1024x512; flat palette colors="..#colors.."\n")
report:close()
sprite:close()
