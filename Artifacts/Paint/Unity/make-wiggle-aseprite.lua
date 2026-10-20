local dir="D:/Github/Engine/Artifacts/Paint/Unity/"
local original=Sprite{fromFile="D:/Github/Engine/Artifacts/Paint/Paint-Parts-v4.aseprite"}
local flat=Image(original)
local fixedPalette=Palette(original.palettes[1])
original:close()
local file=assert(io.open(dir.."offsets128.bin","rb"))
local fields=file:read("*all");file:close()
assert(#fields==3*2*128*128)
local names={"Palette","Brush"}
local anim=Sprite(256,128,ColorMode.RGB)
anim:setPalette(fixedPalette)
anim.layers[1].name="Palette"
local brushLayer=anim:newLayer();brushLayer.name="Brush"
local durations={0.08,0.08,0.09}
for frame=1,3 do
 if frame>1 then anim:newFrame() end
 anim.frames[frame].duration=durations[frame]
 for part=1,2 do
  local source=Image(flat,Rectangle((part-1)*128,0,128,128))
  if frame==1 then
   local still=Image(512,512,ColorMode.RGB)
   for y=0,511 do for x=0,511 do still:drawPixel(x,y,source:getPixel(math.floor(x/4),math.floor(y/4))) end end
   still:saveAs(dir..names[part].."_Static.png")
  end
  local result=Image(128,128,ColorMode.RGB);result:clear()
  for y=0,127 do for x=0,127 do
   local i=((frame-1)*2*128*128)+y*128+x+1
   local dx=fields:byte(i)-1
   local dy=fields:byte(i+128*128)-1
   result:drawPixel(x,y,source:getPixel(math.max(0,math.min(127,x+dx)),math.max(0,math.min(127,y+dy))))
  end end
  local target=anim.layers[part]
  if target:cel(frame) then target:cel(frame).image=result else anim:newCel(target,frame,result,Point((part-1)*128,0)) end
  target:cel(frame).position=Point((part-1)*128,0)
  local out=Image(512,512,ColorMode.RGB)
  for y=0,511 do for x=0,511 do out:drawPixel(x,y,result:getPixel(math.floor(x/4),math.floor(y/4))) end end
  out:saveAs(dir..names[part]..string.format("_%02d.png",frame))
 end
end
anim.data="Paintv4 approvedcolors; native128 integerpixel localwiggle, strength0.8464,3phase,80/80/90ms. No filtered pixel sampling."
anim:saveAs(dir.."Paint_Wiggle.aseprite")
local report=io.open(dir.."aseprite-wiggle-run.txt","w")
report:write("Aseprite "..tostring(app.version).."; native256x128;2parts;3frames;512PNG perpart;80/80/90ms;max1 nativepixel localwarp\n")
report:close();anim:close()
