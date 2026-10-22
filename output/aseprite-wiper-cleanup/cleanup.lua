-- Native indexed-pixel cleanup, executed by the installed Aseprite.
local cleanupOK, cleanupError = xpcall(function()
local root = 'D:/Github/Engine/output/aseprite-wiper-cleanup/'
local input = 'C:/Users/handa/AppData/Local/Temp/codex-clipboard-69557268-4ca4-4a75-a4ec-86d9e81901b4.png'
local W, H = 240, 80
local pc = app.pixelColor
local rgb = {
  {0,0,0,0}, {20,22,30,255},
  {201,203,209,255}, {137,139,150,255},
  {86,90,105,255}, {53,58,75,255},
  {255,107,79,255}, {245,45,53,255},
  {188,28,50,255}, {128,28,50,255}
}
local function material(c)
  if c == 0 then return 0 end
  if c == 1 then return 1 end
  return c <= 5 and 2 or 3
end
local source = assert(app.open(input), 'Cannot open source')
local sourceCel = assert(source.cels[1])
local sourceImage = sourceCel.image
local sourceW, sourceH = source.width, source.height
local function sourcePixel(x,y)
  x, y = x-sourceCel.position.x, y-sourceCel.position.y
  if x<0 or y<0 or x>=sourceImage.width or y>=sourceImage.height then return 0 end
  return sourceImage:getPixel(x,y)
end
local function nearest(r,g,b,first,last)
  local best, distance = first, math.huge
  for c=first,last do
    local p=rgb[c+1]
    local d=(r-p[1])^2+(g-p[2])^2+(b-p[3])^2
    if d<distance then best,distance=c,d end
  end
  return best
end
local function classify(value)
  if pc.rgbaA(value)<128 then return 0 end
  local r,g,b=pc.rgbaR(value),pc.rgbaG(value),pc.rgbaB(value)
  if r+g+b<120 then return 1 end
  if r>g*1.5 and r>b*1.3 then return nearest(r,g,b,6,9) end
  return nearest(r,g,b,2,5)
end
local pixels={}
-- Vote on coverage and material before voting on a shading tone.
-- This prevents anti-aliased edge colors from becoming extra pixels.
for y=0,H-1 do
  local y0=math.floor(y*sourceH/H)
  local y1=math.floor((y+1)*sourceH/H)-1
  for x=0,W-1 do
    local x0=math.floor(x*sourceW/W)
    local x1=math.floor((x+1)*sourceW/W)-1
    local votes,mat,total={},{0,0,0,0},0
    for sy=y0,y1 do for sx=x0,x1 do
      local c=classify(sourcePixel(sx,sy))
      votes[c]=(votes[c] or 0)+1
      local m=material(c)+1
      mat[m]=mat[m]+1
      total=total+1
    end end
    local result=0
    if mat[1]<total/2 then
      local chosen=1
      for m=2,3 do if mat[m+1]>mat[chosen+1] then chosen=m end end
      if chosen==1 then result=1 else
        local first,last=2,5
        if chosen==3 then first,last=6,9 end
        local count=-1
        for c=first,last do
          if (votes[c] or 0)>count then result,count=c,votes[c] or 0 end
        end
      end
    end
    pixels[y*W+x+1]=result
  end
end
local function adjacent(index,diagonal)
  local x=(index-1)%W
  local y=math.floor((index-1)/W)
  local list={}
  for dy=-1,1 do for dx=-1,1 do
    if (dx~=0 or dy~=0) and (diagonal or dx==0 or dy==0) then
      local nx,ny=x+dx,y+dy
      if nx>=0 and nx<W and ny>=0 and ny<H then list[#list+1]=ny*W+nx+1 end
    end
  end end
  return list
end
local function components(test,diagonal)
  local seen,groups={},{}
  for start=1,W*H do
    if not seen[start] and test(pixels[start],start) then
      local queue,head={start},1
      seen[start]=true
      local group={items={},x=W,y=H,right=0,bottom=0}
      while head<=#queue do
        local i=queue[head];head=head+1
        local x,y=(i-1)%W,math.floor((i-1)/W)
        group.items[#group.items+1]=i
        group.x=math.min(group.x,x);group.y=math.min(group.y,y)
        group.right=math.max(group.right,x);group.bottom=math.max(group.bottom,y)
        for _,n in ipairs(adjacent(i,diagonal)) do
          if not seen[n] and test(pixels[n],n) then seen[n]=true;queue[#queue+1]=n end
        end
      end
      groups[#groups+1]=group
    end
  end
  return groups
end
local removedDetached=0
for _,group in ipairs(components(function(c) return c~=0 end,true)) do
  if #group.items<=3 then
    for _,i in ipairs(group.items) do pixels[i]=0;removedDetached=removedDetached+1 end
  end
end
-- Remove only tiny, fully internal tone speckles, never perimeter detail.
local removedSpeckles=0
for pass=1,2 do
  for shade=2,9 do
    local changes={}
    for _,group in ipairs(components(function(c) return c==shade end,false)) do
      if #group.items<=2 then
        local counts,internal={},true
        for _,i in ipairs(group.items) do for _,n in ipairs(adjacent(i,true)) do
          local c=pixels[n]
          if material(c)~=material(shade) then internal=false end
          if c~=shade and material(c)==material(shade) then counts[c]=(counts[c] or 0)+1 end
        end end
        if internal then
          local best,count=nil,0
          for c,n in pairs(counts) do if n>count then best,count=c,n end end
          if best then for _,i in ipairs(group.items) do changes[i]=best end end
        end
      end
    end
    for i,c in pairs(changes) do pixels[i]=c;removedSpeckles=removedSpeckles+1 end
  end
end
-- Restore a complete one-native-pixel dark perimeter where input edges broke.
local perimeter={}
for i,c in ipairs(pixels) do
  if c~=0 then
    local x,y=(i-1)%W,math.floor((i-1)/W)
    local boundary=x==0 or x==W-1 or y==0 or y==H-1
    for _,n in ipairs(adjacent(i,false)) do if pixels[n]==0 then boundary=true end end
    if boundary then perimeter[#perimeter+1]=i end
  end
end
for _,i in ipairs(perimeter) do pixels[i]=1 end
local bodies=components(function(c) return c~=0 end,true)
table.sort(bodies,function(a,b) return a.x<b.x end)
assert(#bodies==5,'Expected five connected monsters, found '..#bodies)
-- The only small disconnected ink components inside a body are its two eyes
-- and mouth. Redraw these on exact integer coordinates at native resolution.
local faceFeatures={}
for _,group in ipairs(components(function(c) return c==1 end,false)) do
  if #group.items<=20 then
    local internal=true
    for _,i in ipairs(group.items) do for _,n in ipairs(adjacent(i,true)) do
      if pixels[n]==0 then internal=false end
    end end
    if internal then faceFeatures[#faceFeatures+1]=group end
  end
end
local faces={}
for _,body in ipairs(bodies) do
  local features={}
  for _,feature in ipairs(faceFeatures) do
    if feature.x>=body.x and feature.right<=body.right and feature.y>=body.y and feature.bottom<=body.bottom then
      features[#features+1]=feature
    end
  end
  local featureDetails={}
  for _,f in ipairs(features) do featureDetails[#featureDetails+1]=f.x..','..f.y..':'..#f.items end
  local selected,bestScore=nil,math.huge
  for a=1,#features do for b=a+1,#features do
    local left,right=features[a],features[b]
    if left.x>right.x then left,right=right,left end
    local lx,ly=(left.x+left.right)/2,(left.y+left.bottom)/2
    local rx,ry=(right.x+right.right)/2,(right.y+right.bottom)/2
    if math.abs(ly-ry)<=1.5 and rx-lx>=4 and rx-lx<=14 then
      for m,mouth in ipairs(features) do if m~=a and m~=b then
        local mx,my=(mouth.x+mouth.right)/2,(mouth.y+mouth.bottom)/2
        local dy=my-(ly+ry)/2
        if mx>lx and mx<rx and dy>=1 and dy<=6 then
          local score=math.abs(ly-ry)*3+math.abs(mx-(lx+rx)/2)+math.abs(dy-3)
          if score<bestScore then selected,bestScore={left,right,mouth},score end
        end
      end end
    end
  end end
  assert(selected,'Cannot locate two eyes and mouth at body x='..body.x..' ['..table.concat(featureDetails,';')..']')
  for _,feature in ipairs(features) do
    local retained=false
    for _,keep in ipairs(selected) do if keep==feature then retained=true end end
    if not retained then
      local counts={}
      for _,i in ipairs(feature.items) do for _,n in ipairs(adjacent(i,true)) do
        local c=pixels[n]
        if c>=2 and c<=5 then counts[c]=(counts[c] or 0)+1 end
      end end
      local fill,count=3,-1
      for c,n in pairs(counts) do if n>count then fill,count=c,n end end
      for _,i in ipairs(feature.items) do pixels[i]=fill end
      removedSpeckles=removedSpeckles+#feature.items
    end
  end
  features=selected
  table.sort(features,function(a,b) return a.y<b.y end)
  for order,feature in ipairs(features) do
    local counts={}
    for _,i in ipairs(feature.items) do for _,n in ipairs(adjacent(i,true)) do
      local c=pixels[n]
      if c>=2 and c<=5 then counts[c]=(counts[c] or 0)+1 end
    end end
    local fill,count=3,-1
    for c,n in pairs(counts) do if n>count then fill,count=c,n end end
    for _,i in ipairs(feature.items) do pixels[i]=fill end
    local size=order==3 and 1 or 2
    local x=math.floor((feature.x+feature.right-size+1)/2)
    local y=math.floor((feature.y+feature.bottom-size+1)/2)
    for dy=0,size-1 do for dx=0,size-1 do pixels[(y+dy)*W+x+dx+1]=1 end end
    faces[#faces+1]={x=x,y=y,size=size}
  end
end
local used={}
for _,c in ipairs(pixels) do used[c]=true end
local opaqueColors=0
for c in pairs(used) do if c~=0 then opaqueColors=opaqueColors+1 end end
local output=Sprite(W,H,ColorMode.INDEXED)
output.transparentColor=0
local palette=Palette(#rgb)
for i,c in ipairs(rgb) do palette:setColor(i-1,Color{r=c[1],g=c[2],b=c[3],a=c[4]}) end
output:setPalette(palette)
local names={'01 Small','02 Defender','03 Charger','04 Tall','05 Heavy'}
for number,body in ipairs(bodies) do
  local layer=number==1 and output.layers[1] or output:newLayer()
  layer.name=names[number]
  local image=Image(W,H,ColorMode.INDEXED)
  for _,i in ipairs(body.items) do
    local x,y=(i-1)%W,math.floor((i-1)/W)
    image:drawPixel(x,y,pixels[i])
  end
  local cel=layer:cel(1)
  if cel then cel.image=image;cel.position=Point(0,0) else output:newCel(layer,1,image,Point(0,0)) end
end
assert(output:saveAs(root..'wiper-cleaned.aseprite'))
assert(output:saveAs(root..'wiper-cleaned-native.png'))
local report=assert(io.open(root..'cleanup-report.json','w'))
report:write(string.format('{"asepriteVersion":"%s","sourceWidth":%d,"sourceHeight":%d,"width":%d,"height":%d,"layers":%d,"monsters":%d,"opaqueColors":%d,"paletteSize":%d,"eyes":10,"mouths":5,"eyeSize":"2x2","mouthSize":"1x1","removedDetachedPixels":%d,"removedInternalSpeckles":%d,"colorMode":"indexed","alphaValues":[0,255]}',tostring(app.version),sourceW,sourceH,W,H,#output.layers,#bodies,opaqueColors,#rgb,removedDetached,removedSpeckles))
report:close()
output:close()
source:close()
end, debug.traceback)
if not cleanupOK then
  local errorFile=assert(io.open('D:/Github/Engine/output/aseprite-wiper-cleanup/cleanup-error.txt','w'))
  errorFile:write(tostring(cleanupError))
  errorFile:close()
end
