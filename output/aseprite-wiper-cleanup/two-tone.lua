local root='D:/Github/Engine/output/aseprite-wiper-cleanup/'
local ok,err=xpcall(function()
  local sprite=assert(app.open(root..'wiper-cleaned.aseprite'))
  assert(sprite.colorMode==ColorMode.INDEXED)
  assert(sprite.width==240 and sprite.height==80 and #sprite.layers==5)
  assert(not app.fs.isFile(root..'wiper-two-tone.aseprite'),'Output already exists')
  -- Two flat tones per material; outline and facial pixels stay unchanged.
  local remap={[0]=0,[1]=1,[2]=2,[3]=2,[4]=3,[5]=3,[6]=4,[7]=4,[8]=5,[9]=5}
  for _,cel in ipairs(sprite.cels) do
    for pixel in cel.image:pixels() do
      local value=assert(remap[pixel()],'Unexpected palette index')
      pixel(value)
    end
  end
  local colors={
    {0,0,0,0}, {20,22,30,255},
    {160,162,172,255}, {103,108,125,255},
    {240,52,59,255}, {185,40,59,255}
  }
  local palette=Palette(#colors)
  for i,c in ipairs(colors) do palette:setColor(i-1,Color{r=c[1],g=c[2],b=c[3],a=c[4]}) end
  sprite:setPalette(palette)
  sprite.transparentColor=0
  assert(sprite:saveAs(root..'wiper-two-tone.aseprite'))
  assert(sprite:saveAs(root..'wiper-two-tone-native.png'))
  sprite:close()
  local original=assert(app.open(root..'wiper-cleaned.aseprite'))
  local output=assert(app.open(root..'wiper-two-tone.aseprite'))
  assert(output.colorMode==ColorMode.INDEXED and #output.palettes[1]==6)
  assert(output.width==original.width and output.height==original.height)
  assert(#output.layers==5 and #output.frames==1 and output.transparentColor==0)
  local function read(layer,x,y)
    local cel=layer:cel(1)
    if not cel then return 0 end
    x,y=x-cel.position.x,y-cel.position.y
    if x<0 or y<0 or x>=cel.image.width or y>=cel.image.height then return 0 end
    return cel.image:getPixel(x,y)
  end
  local checked,ink,opaque,used=0,0,0,{}
  for layer=1,5 do
    assert(output.layers[layer].name==original.layers[layer].name)
    for y=0,output.height-1 do for x=0,output.width-1 do
      local old=read(original.layers[layer],x,y)
      local current=read(output.layers[layer],x,y)
      assert(current==remap[old],'Unexpected pixel change')
      if old==1 then ink=ink+1 end
      if current~=0 then opaque=opaque+1 end
      used[current]=true
      checked=checked+1
    end end
  end
  local usedCount=0
  for _ in pairs(used) do usedCount=usedCount+1 end
  assert(usedCount==6,'Expected five opaque colors plus transparency')
  for i=0,5 do assert(output.palettes[1]:getColor(i).alpha==(i==0 and 0 or 255)) end
  local report=assert(io.open(root..'two-tone-verification.json','w'))
  report:write(string.format('{"asepriteVersion":"%s","width":240,"height":80,"layers":5,"grayTones":2,"redTones":2,"outlineColors":1,"paletteEntries":6,"reopenedAndVerified":true,"checkedLayerPixels":%d,"unchangedOutlineAndFacePixels":%d,"opaquePixels":%d,"pixelErrors":0,"alphaValues":[0,255]}',tostring(app.version),checked,ink,opaque))
  report:close()
  original:close()
  output:close()
end,debug.traceback)
if not ok then
  local report=assert(io.open(root..'two-tone-error.txt','w'))
  report:write(tostring(err));report:close()
end
