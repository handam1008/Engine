-- Slice and register the generated crescent animation, then normalize it
-- to the existing Paint art's integer 2px native clusters and fixed palette.
local root="D:/Github/Engine/"
local work=root.."Artifacts/Paint/PaintArc/"
local dest=root.."Assets/RYU/Sprite/Charcter/Paint/Effects/PaintArc/"
local source=Image{fromFile=work.."PaintArc-Red-generated.png"}
assert(source.width==1983 and source.height==793,"Unexpected source dimensions")
local pc=app.pixelColor
local palettes={
 {name="Red",colors={{112,29,31},{164,52,51},{238,67,59},{255,140,103}}},
 {name="Yellow",colors={{124,84,18},{194,144,35},{248,200,48},{255,234,130}}},
 {name="Blue",colors={{22,57,108},{45,97,163},{59,148,224},{140,211,247}}}
}
-- All five frames use the same 512px view around the inferred center of
-- their empty interior. Separate boundaries prevent neighboring-cell bleed.
local boundaries={0,405,782,1175,1565,1983}
local centers={247,631,1015,1399,1783}
local fields={}
local report={}
for frame=1,5 do
 local cells={}
 for by=0,63 do for bx=0,63 do
  local totalAlpha,r,g,b=0,0,0,0
  local sx=centers[frame]-256+bx*8
  local sy=390-256+by*8
  for dy=0,7 do for dx=0,7 do
   local x,y=sx+dx,sy+dy
   if x>=boundaries[frame] and x<boundaries[frame+1] and y>=0 and y<source.height then
    local value=source:getPixel(x,y)
    local alpha=pc.rgbaA(value)
    if alpha>=128 then
     totalAlpha=totalAlpha+alpha
     r=r+pc.rgbaR(value)*alpha
     g=g+pc.rgbaG(value)*alpha
     b=b+pc.rgbaB(value)*alpha
    end
   end
  end end
  local index=0
  if totalAlpha>=64*255*0.4 then
   r,g,b=r/totalAlpha,g/totalAlpha,b/totalAlpha
   local best=math.huge
   for i,c in ipairs(palettes[1].colors) do
    local distance=(r-c[1])^2+(g-c[2])^2+(b-c[3])^2
    if distance<best then index=i;best=distance end
   end
  end
  cells[by*64+bx+1]=index
 end end
 -- Discard single-grid speckles; retain the intended curved fragments.
 local seen={}
 local components={}
 for start=1,4096 do
  if cells[start]>0 and not seen[start] then
   local queue={start};seen[start]=true
   local head=1
   while head<=#queue do
    local i=queue[head];head=head+1
    local x,y=(i-1)%64,math.floor((i-1)/64)
    for dy=-1,1 do for dx=-1,1 do
     local nx,ny=x+dx,y+dy
     local j=ny*64+nx+1
     if nx>=0 and nx<64 and ny>=0 and ny<64 and cells[j]>0 and not seen[j] then seen[j]=true;queue[#queue+1]=j end
    end end
   end
   if #queue<4 then for _,i in ipairs(queue) do cells[i]=0 end else components[#components+1]=#queue end
  end
 end
 fields[frame]=cells
 report[#report+1]="Frame "..frame.." component grid areas: "..table.concat(components,",")
end
local contact=Image(2560,1536,ColorMode.RGB);contact:clear()
local preview=Sprite(384,128,ColorMode.RGB)
preview.layers[1].name="Red Yellow Blue Preview"
for frame=1,6 do
 if frame>1 then preview:newFrame() end
 preview.frames[frame].duration=frame==6 and 0.32 or 0.04
 local image=Image(384,128,ColorMode.RGB);image:clear()
 if preview.layers[1]:cel(frame) then preview.layers[1]:cel(frame).image=image else preview:newCel(preview.layers[1],frame,image,Point(0,0)) end
end
for color,palette in ipairs(palettes) do
 local native=Sprite(128,128,ColorMode.RGB)
 native.layers[1].name="PaintArc_"..palette.name
 for frame=1,5 do
  if frame>1 then native:newFrame() end
  native.frames[frame].duration=0.04
  local small=Image(128,128,ColorMode.RGB);small:clear()
  for by=0,63 do for bx=0,63 do
   local index=fields[frame][by*64+bx+1]
   if index>0 then
    local c=palette.colors[index]
    for dy=0,1 do for dx=0,1 do small:drawPixel(bx*2+dx,by*2+dy,pc.rgba(c[1],c[2],c[3],255)) end end
   end
  end end
  local cel=native.layers[1]:cel(frame)
  if cel then cel.image=small;cel.position=Point(0,0) else native:newCel(native.layers[1],frame,small,Point(0,0)) end
  local png=Image(512,512,ColorMode.RGB);png:clear()
  for y=0,511 do for x=0,511 do png:drawPixel(x,y,small:getPixel(math.floor(x/4),math.floor(y/4))) end end
  png:saveAs(dest.."PaintArc_"..palette.name..string.format("_%02d.png",frame))
  contact:drawImage(png,Point((frame-1)*512,(color-1)*512))
  local target=preview.layers[1]:cel(frame).image:clone()
  target:drawImage(small,Point((color-1)*128,0))
  preview.layers[1]:cel(frame).image=target
 end
 native.data="Generated paint arc; five registered frames,40ms each; source128px,2x2 native clusters, nearest4x PNG512;4 opaque palette colors; same geometry for all colors."
 native:saveAs(work.."PaintArc_"..palette.name..".aseprite")
 native:close()
end
contact:saveAs(work.."PaintArc-contact.png")
preview:saveAs(work.."PaintArc-preview.aseprite")
preview:close()
local f=assert(io.open(work.."aseprite-preparation.txt","w"))
f:write("Aseprite "..tostring(app.version).."\n15 PNG frames512x512, native128x128,5frames at40ms, fixedpivot center\n"..table.concat(report,"\n").."\n")
f:close()
