local root='D:/Github/Engine/output/aseprite-wiper-cleanup/'
local sprite=assert(app.open(root..'wiper-cleaned.aseprite'))
assert(sprite.width==240 and sprite.height==80,'Incorrect native dimensions')
assert(sprite.colorMode==ColorMode.INDEXED,'Not an indexed-pixel document')
assert(#sprite.layers==5,'Incorrect layer count')
assert(#sprite.frames==1,'Incorrect frame count')
assert(sprite.transparentColor==0,'Incorrect transparency index')
local palette=sprite.palettes[1]
assert(#palette==10,'Incorrect palette size')
assert(palette:getColor(0).alpha==0,'Transparency entry is not transparent')
for i=1,9 do assert(palette:getColor(i).alpha==255,'Semitransparent palette entry') end
local pixelCount=0
for _,cel in ipairs(sprite.cels) do
  for it in cel.image:pixels() do
    local index=it()
    assert(index>=0 and index<=9,'Pixel outside restricted palette')
    pixelCount=pixelCount+1
  end
end
local report=assert(io.open(root..'aseprite-verification.json','w'))
report:write(string.format('{"reopenedByAseprite":true,"indexed":true,"width":240,"height":80,"layers":5,"frames":1,"paletteEntries":10,"semitransparentPixels":0,"checkedLayerPixels":%d}',pixelCount))
report:close()
sprite:close()
