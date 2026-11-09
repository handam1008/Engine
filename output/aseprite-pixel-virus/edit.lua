local root = 'D:/Github/Engine/output/aseprite-pixel-virus/'
local source = 'D:/Github/Engine/output/aseprite-pixel-cat/pixel-cat.aseprite'

local ok, err = xpcall(function()
  local sprite = assert(app.open(source))
  assert(sprite.width==32 and sprite.height==32)
  assert(sprite.colorMode==ColorMode.INDEXED)
  local original = sprite.layers[1]:cel(1).image
  local mask = {}
  for y=8,31 do
    for x=0,31 do
      if original:getPixel(x,y)~=0 then mask[y*32+x]=true end
    end
  end

  -- Replace just the triangular ears with two short, round-tipped antennae.
  local antennaRows = {
    [2]={{8,10},{21,23}},
    [3]={{7,11},{20,24}},
    [4]={{7,11},{20,24}},
    [5]={{8,10},{21,23}},
    [6]={{8,10},{21,23}},
    [7]={{8,10},{21,23}},
  }
  for y,runs in pairs(antennaRows) do
    for _,run in ipairs(runs) do
      for x=run[1],run[2] do mask[y*32+x]=true end
    end
  end
  local function inside(x,y)
    return x>=0 and x<32 and y>=0 and y<32 and mask[y*32+x]
  end
  local body = Image(32,32,ColorMode.INDEXED)
  for y=0,31 do
    for x=0,31 do
      if inside(x,y) then
        local edge = not inside(x-1,y) or not inside(x+1,y)
          or not inside(x,y-1) or not inside(x,y+1)
        body:drawPixel(x,y,edge and 1 or 2)
      end
      if y>=9 then assert(body:getPixel(x,y)==original:getPixel(x,y)) end
    end
  end
  sprite.layers[1]:cel(1).image = body
  sprite.layers[1].name = 'Body - flat lime and rounded antennae'
  local palette = Palette(3)
  palette:setColor(0,Color{r=0,g=0,b=0,a=0})
  palette:setColor(1,Color{r=31,g=36,b=46,a=255})
  palette:setColor(2,Color{r=116,g=186,b=58,a=255})
  sprite:setPalette(palette)

  assert(sprite:saveAs(root..'pixel-virus.aseprite'))
  assert(sprite:saveAs(root..'pixel-virus-32.png'))
  sprite:close()

  local reopened = assert(app.open(root..'pixel-virus.aseprite'))
  assert(reopened.width==32 and reopened.height==32)
  assert(reopened.colorMode==ColorMode.INDEXED)
  assert(#reopened.frames==1 and #reopened.layers==2)
  assert(#reopened.palettes[1]==3)
  local dots = 0
  for px in reopened.layers[2]:cel(1).image:pixels() do
    if px()~=0 then assert(px()==1); dots=dots+1 end
  end
  assert(dots==3)
  local report = assert(io.open(root..'aseprite-check.txt','w'))
  report:write('PASS\nCanvas: 32x32\nIndexed palette: transparent, #1F242E, #74BA3A\nFrames: 1\nFace: original three single-pixel dots\nBody and feet: original pixels preserved from row 9 down\nEdit: triangular ears replaced with rounded antennae\n')
  report:close()
  reopened:close()
end,debug.traceback)

if not ok then
  local report=assert(io.open(root..'error.txt','w'))
  report:write(tostring(err)); report:close()
  error(err)
end
