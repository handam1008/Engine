local root='D:/Github/Engine/output/aseprite-wiper-cleanup/'
local ok,err=xpcall(function()
  local sprite=assert(app.open(root..'wiper-two-tone.aseprite'))
  assert(sprite.colorMode==ColorMode.INDEXED and #sprite.layers==5)
  local remap={[0]=0,[1]=1,[2]=2,[3]=2,[4]=3,[5]=3}
  for _,cel in ipairs(sprite.cels) do for pixel in cel.image:pixels() do pixel(assert(remap[pixel()])) end end
  local palette=Palette(4)
  palette:setColor(0,Color{r=0,g=0,b=0,a=0})
  palette:setColor(1,Color{r=20,g=22,b=30,a=255})
  palette:setColor(2,Color{r=160,g=162,b=172,a=255})
  palette:setColor(3,Color{r=240,g=52,b=59,a=255})
  sprite:setPalette(palette)
  sprite.transparentColor=0
  local dump=assert(io.open(root..'shape-reference.txt','w'))
  local chars={[0]='.',[1]='#',[2]='G',[3]='R'}
  for _,layer in ipairs(sprite.layers) do
    local cel=layer:cel(1)
    local image=cel.image
    local left,top,right,bottom=sprite.width,sprite.height,-1,-1
    for y=0,image.height-1 do for x=0,image.width-1 do
      if image:getPixel(x,y)~=0 then
        left=math.min(left,x+cel.position.x);top=math.min(top,y+cel.position.y)
        right=math.max(right,x+cel.position.x);bottom=math.max(bottom,y+cel.position.y)
      end
    end end
    dump:write(string.format('\n%s X=%d..%d Y=%d..%d\n    ',layer.name,left,right,top,bottom))
    for x=left,right do dump:write(tostring(x%10)) end
    dump:write('\n')
    for y=top,bottom do
      dump:write(string.format('%02d  ',y))
      for x=left,right do
        local ix,iy=x-cel.position.x,y-cel.position.y
        local c=0
        if ix>=0 and iy>=0 and ix<image.width and iy<image.height then c=image:getPixel(ix,iy) end
        dump:write(chars[c])
      end
      dump:write('\n')
    end
  end
  dump:close()
  assert(sprite:saveAs(root..'wiper-flat-polish-draft.aseprite'))
  assert(sprite:saveAs(root..'wiper-flat-polish-draft.png'))
  sprite:close()
end,debug.traceback)
if not ok then local f=assert(io.open(root..'polish-error.txt','w'));f:write(tostring(err));f:close() end
