-- Native 32x32 prototype. Coordinates below are source pixels, not traced art.
local root = 'D:/Github/Engine/output/aseprite-pixel-pest/'

local ok, err = xpcall(function()
  local sprite = Sprite(32, 32, ColorMode.INDEXED)
  sprite.transparentColor = 0
  local palette = Palette(3)
  palette:setColor(0, Color{r=0, g=0, b=0, a=0})
  palette:setColor(1, Color{r=31, g=36, b=46, a=255})
  palette:setColor(2, Color{r=116, g=186, b=58, a=255})
  sprite:setPalette(palette)

  -- Broad, low bean: a forward cheek, a rounded back and two short feet.
  local spans = {
    [9]={{11,19}},
    [10]={{8,22}},
    [11]={{6,24}},
    [12]={{5,25}},
    [13]={{4,26}},
    [14]={{4,27}},
    [15]={{3,27}},
    [16]={{3,27}},
    [17]={{3,27}},
    [18]={{3,27}},
    [19]={{4,27}},
    [20]={{4,26}},
    [21]={{5,25}},
    [22]={{6,24}},
    [23]={{8,23}},
    [24]={{7,12},{21,25}},
    [25]={{7,12},{21,25}},
    [26]={{6,12},{20,25}},
  }
  local mask = {}
  for y, runs in pairs(spans) do
    for _, run in ipairs(runs) do
      for x=run[1],run[2] do mask[y*32+x] = true end
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
    end
  end
  local bodyLayer = sprite.layers[1]
  bodyLayer.name = 'Body - flat green and ink'
  bodyLayer:cel(1).image = body

  local face = Image(32,32,ColorMode.INDEXED)
  for y=15,16 do
    for x=8,9 do face:drawPixel(x,y,1) end
    for x=14,15 do face:drawPixel(x,y,1) end
  end
  face:drawPixel(11,19,1)
  local faceLayer = sprite:newLayer()
  faceLayer.name = 'Face - two square eyes and dot mouth'
  sprite:newCel(faceLayer,1,face,Point(0,0))

  assert(sprite:saveAs(root..'pixel-pest.aseprite'))
  assert(sprite:saveAs(root..'pixel-pest-32.png'))
  sprite:close()

  local reopened = assert(app.open(root..'pixel-pest.aseprite'))
  assert(reopened.width==32 and reopened.height==32)
  assert(reopened.colorMode==ColorMode.INDEXED)
  assert(#reopened.frames==1 and #reopened.layers==2)
  assert(#reopened.palettes[1]==3)
  local report = assert(io.open(root..'aseprite-check.txt','w'))
  report:write('Native canvas: 32x32\nColor mode: indexed\nPalette: transparent + ink + green\nFrames: 1\nLayers: body + face\n')
  report:close()
  reopened:close()
end, debug.traceback)

if not ok then
  local report = assert(io.open(root..'error.txt','w'))
  report:write(tostring(err))
  report:close()
  error(err)
end
