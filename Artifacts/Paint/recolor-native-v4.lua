-- Color-only revision of the existing editable Aseprite document.
-- Every cel, coordinate, shape and alpha value remains identical to v3.
local base="D:/Github/Engine/Artifacts/Paint/"
local sprite=Sprite{fromFile=base.."Paint-Parts-v3.aseprite"}
local pc=app.pixelColor
local changes={
  {{177,132,84},{166,115,69}},
  {{219,180,119},{226,176,100}},
  {{197,156,100},{203,145,80}},
  {{243,215,161},{255,224,157}},
  {{157,70,65},{164,52,51}},
  {{220,82,72},{238,67,59}},
  {{245,147,115},{255,140,103}},
  {{178,137,51},{194,144,35}},
  {{230,188,65},{248,200,48}},
  {{250,222,128},{255,234,130}},
  {{64,105,148},{45,97,163}},
  {{100,155,204},{59,148,224}},
  {{164,210,234},{140,211,247}},
  {{79,132,181},{63,139,224}},
  {{49,83,122},{40,77,145}},
  {{147,196,223},{144,207,249}}
}
local map={}
for _,pair in ipairs(changes) do local a,b=pair[1],pair[2];map[pc.rgba(a[1],a[2],a[3],255)]=pc.rgba(b[1],b[2],b[3],255) end
local changed=0
for _,cel in ipairs(sprite.cels) do
  local image=cel.image:clone()
  for pixel in image:pixels() do local value=map[pixel()];if value then pixel(value);changed=changed+1 end end
  cel.image=image
end
for paletteIndex=1,#sprite.palettes do
  local palette=sprite.palettes[paletteIndex]
  for i=0,#palette-1 do local c=palette:getColor(i);local value=map[pc.rgba(c.red,c.green,c.blue,c.alpha)];if value then palette:setColor(i,value) end end
end
sprite.data=sprite.data.." Color revision v4: livelier red/yellow/blue, warmer honey wood; unchanged geometry, outline, metal and unpainted bristles."
sprite:saveAs(base.."Paint-Parts-v4.aseprite")
local flat=Image(sprite)
flat:saveAs(base.."Paint-Parts-v4-logical.png")
local exported=Image(1024,512,ColorMode.RGB)
for y=0,511 do for x=0,1023 do exported:drawPixel(x,y,flat:getPixel(math.floor(x/4),math.floor(y/4))) end end
exported:saveAs(base.."Paint-Parts-v4.png")
local report=io.open(base.."Paint-Parts-v4-aseprite-run.txt","w")
report:write("Aseprite "..tostring(app.version).."\nNative256x128; percell128x128; integer clusters2x2; PNG1024x512; color-only remap16colors; celpixelschanged="..changed.."\n")
report:close()
sprite:close()
