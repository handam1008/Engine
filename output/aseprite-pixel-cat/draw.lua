local root = 'D:/Github/Engine/output/aseprite-pixel-cat/'

local ok, err = xpcall(function()
  local sprite = Sprite(32, 32, ColorMode.INDEXED)
  sprite.transparentColor = 0
  local palette = Palette(3)
  palette:setColor(0, Color{r=0,g=0,b=0,a=0})
  palette:setColor(1, Color{r=31,g=36,b=46,a=255})
  palette:setColor(2, Color{r=255,g=255,b=255,a=255})
  sprite:setPalette(palette)

  -- Native-pixel silhouette: two small ears, one round body, two feet.
  local rows = {
    [4]={{9,9},{23,23}},
    [5]={{9,10},{22,23}},
    [6]={{8,11},{21,24}},
    [7]={{8,12},{20,24}},
    [8]={{8,24}},
    [9]={{7,25}},
    [10]={{6,26}},
    [11]={{5,26}},
    [12]={{5,27}},
    [13]={{4,27}},
    [14]={{4,27}},
    [15]={{4,27}},
    [16]={{4,27}},
    [17]={{4,27}},
    [18]={{4,27}},
    [19]={{4,27}},
    [20]={{5,27}},
    [21]={{5,26}},
    [22]={{6,26}},
    [23]={{7,25}},
    [24]={{8,24}},
    [25]={{10,22}},
    [26]={{10,12},{21,23}},
    [27]={{9,12},{21,24}},
    [28]={{9,12},{21,24}},
  }
  local mask = {}
  for y, runs in pairs(rows) do
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
        local boundary = not inside(x-1,y) or not inside(x+1,y)
          or not inside(x,y-1) or not inside(x,y+1)
        body:drawPixel(x,y,boundary and 1 or 2)
      end
    end
  end
  local bodyLayer = sprite.layers[1]
  bodyLayer.name = 'Body - white and one-pixel ink outline'
  bodyLayer:cel(1).image = body

  local face = Image(32,32,ColorMode.INDEXED)
  face:drawPixel(10,15,1)
  face:drawPixel(16,15,1)
  face:drawPixel(13,18,1)
  local faceLayer = sprite:newLayer()
  faceLayer.name = 'Face - three single-pixel dots'
  sprite:newCel(faceLayer,1,face,Point(0,0))

  assert(sprite:saveAs(root..'pixel-cat.aseprite'))
  assert(sprite:saveAs(root..'pixel-cat-32.png'))
  sprite:close()

  local reopened = assert(app.open(root..'pixel-cat.aseprite'))
  assert(reopened.width==32 and reopened.height==32)
  assert(reopened.colorMode==ColorMode.INDEXED)
  assert(#reopened.frames==1 and #reopened.layers==2)
  assert(#reopened.palettes[1]==3)
  local facePixelCount = 0
  for px in reopened.layers[2]:cel(1).image:pixels() do
    if px()~=0 then
      assert(px()==1)
      facePixelCount=facePixelCount+1
    end
  end
  assert(facePixelCount==3)
  local report = assert(io.open(root..'aseprite-check.txt','w'))
  report:write('PASS\nNative canvas: 32x32\nColor mode: indexed\nPalette: transparent, #1F242E, #FFFFFF\nFrames: 1\nLayers: body and face\nFace: three single-pixel dots\n')
  report:close()
  reopened:close()
end, debug.traceback)

if not ok then
  local report=assert(io.open(root..'error.txt','w'))
  report:write(tostring(err))
  report:close()
  error(err)
end
