local root='D:/Github/Engine/output/virus-reference-edit/'
local input='C:/Users/handa/.codex/generated_images/01a15799-24ac-7233-a8e0-963eac1d169c/exec-45ee08d6-7e98-485c-9985-b4494e7473cc.png'

local ok,err=xpcall(function()
  local source=assert(app.open(input))
  assert(source.colorMode==ColorMode.RGB)
  local width,height=source.width,source.height
  local cel=source.layers[1]:cel(1)
  assert(cel.position.x==0 and cel.position.y==0)
  local src=cel.image
  assert(src.width==width and src.height==height)
  local output=Sprite(width,height,ColorMode.INDEXED)
  output.transparentColor=0
  local palette=Palette(3)
  palette:setColor(0,Color{r=0,g=0,b=0,a=0})
  palette:setColor(1,Color{r=31,g=36,b=46,a=255})
  palette:setColor(2,Color{r=116,g=186,b=58,a=255})
  output:setPalette(palette)
  local image=Image(width,height,ColorMode.INDEXED)
  local counts={0,0,0}
  for y=0,height-1 do
    for x=0,width-1 do
      local pixel=src:getPixel(x,y)
      local alpha=app.pixelColor.rgbaA(pixel)
      local index=0
      if alpha>=128 then
        local r=app.pixelColor.rgbaR(pixel)
        local g=app.pixelColor.rgbaG(pixel)
        local b=app.pixelColor.rgbaB(pixel)
        local ink=(r-31)^2+(g-36)^2+(b-46)^2
        local green=(r-116)^2+(g-186)^2+(b-58)^2
        index=ink<=green and 1 or 2
      end
      image:drawPixel(x,y,index)
      counts[index+1]=counts[index+1]+1
    end
  end
  output.layers[1].name='Attached reference edit - flat green and ink'
  output.layers[1]:cel(1).image=image
  assert(output:saveAs(root..'virus-reference-flat.aseprite'))
  assert(output:saveAs(root..'virus-reference-flat.png'))
  output:close()
  source:close()
  local report=assert(io.open(root..'check.txt','w'))
  report:write(string.format('Canvas: %dx%d (not resized)\nOpaque colors: #1F242E, #74BA3A\nAlpha: 0 or 255 only\nTransparent pixels: %d\nInk pixels: %d\nGreen pixels: %d\nNo silhouette reconstruction or downsampling.\n',width,height,counts[1],counts[2],counts[3]))
  report:close()
end,debug.traceback)
if not ok then
  local file=assert(io.open(root..'error.txt','w'))
  file:write(tostring(err)); file:close()
  error(err)
end
