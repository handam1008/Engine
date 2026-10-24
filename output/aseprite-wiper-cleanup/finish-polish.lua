local root='D:/Github/Engine/output/aseprite-wiper-cleanup/'
local ok,err=xpcall(function()
  local input=assert(app.open(root..'wiper-flat-polish-draft.aseprite'))
  local W,H,PAD=240,80,8
  assert(input.width==W and input.height==H and #input.layers==5)
  assert(not app.fs.isFile(root..'wiper-flat-polished.aseprite'),'Output already exists')
  local output=Sprite(W+PAD*2,H,ColorMode.INDEXED)
  output.transparentColor=0
  output:setPalette(input.palettes[1])
  local changes,trimmed,feet,nubs,protectedCount,maskErrors=0,0,0,0,0,0
  local function neighbors(i,diagonal)
    local x,y=(i-1)%W,math.floor((i-1)/W)
    local result={}
    for dy=-1,1 do for dx=-1,1 do
      if (dx~=0 or dy~=0) and (diagonal or dx==0 or dy==0) then
        local nx,ny=x+dx,y+dy
        if nx>=0 and nx<W and ny>=0 and ny<H then result[#result+1]=ny*W+nx+1 end
      end
    end end
    return result
  end
  for number,layer in ipairs(input.layers) do
    local cel=layer:cel(1)
    local pixels,before={},{}
    for y=0,H-1 do for x=0,W-1 do
      local ix,iy=x-cel.position.x,y-cel.position.y
      local c=0
      if ix>=0 and iy>=0 and ix<cel.image.width and iy<cel.image.height then c=cel.image:getPixel(ix,iy) end
      local i=y*W+x+1;pixels[i]=c;before[i]=c
    end end
    local function components(color)
      local seen,result={},{}
      for start=1,W*H do
        if pixels[start]==color and not seen[start] then
          local queue,head={start},1
          seen[start]=true
          local g={items={},x=W,y=H,right=0,bottom=0}
          while head<=#queue do
            local i=queue[head];head=head+1
            local x,y=(i-1)%W,math.floor((i-1)/W)
            g.items[#g.items+1]=i
            g.x=math.min(g.x,x);g.y=math.min(g.y,y)
            g.right=math.max(g.right,x);g.bottom=math.max(g.bottom,y)
            for _,n in ipairs(neighbors(i,false)) do
              if not seen[n] and pixels[n]==color then seen[n]=true;queue[#queue+1]=n end
            end
          end
          result[#result+1]=g
        end
      end
      return result
    end
    local protected={}
    for _,g in ipairs(components(1)) do
      if #g.items<=4 then
        local internal=true
        for _,i in ipairs(g.items) do for _,n in ipairs(neighbors(i,true)) do if pixels[n]==0 then internal=false end end end
        if internal then for _,i in ipairs(g.items) do protected[i]=true;protectedCount=protectedCount+1 end end
      end
    end
    local footParts={}
    for _,g in ipairs(components(2)) do if g.y>=64 and #g.items<100 then footParts[#footParts+1]=g end end
    assert(#footParts==2,'Expected two feet on '..layer.name)
    local nubParts={}
    for _,g in ipairs(components(3)) do
      if #g.items<50 and g.right-g.x<9 and g.bottom-g.y<9 then nubParts[#nubParts+1]=g end
    end
    -- Trim only excess inner outline thickness; never move the outer mask.
    local distance,queue,head={},{},1
    for i,c in ipairs(pixels) do if c~=0 then
      local x,y=(i-1)%W,math.floor((i-1)/W)
      local edge=x==0 or y==0 or x==W-1 or y==H-1
      for _,n in ipairs(neighbors(i,true)) do if pixels[n]==0 then edge=true end end
      if edge then distance[i]=1;queue[#queue+1]=i end
    end end
    while head<=#queue do
      local i=queue[head];head=head+1
      for _,n in ipairs(neighbors(i,true)) do
        if pixels[n]~=0 and not distance[n] then distance[n]=distance[i]+1;queue[#queue+1]=n end
      end
    end
    for i,c in ipairs(pixels) do
      if c==1 and distance[i]>2 and not protected[i] then pixels[i]=2;trimmed=trimmed+1 end
    end
    -- Resolve the old irregular red tips into clean symmetric pixel clusters.
    for _,g in ipairs(nubParts) do
      local w,h=g.right-g.x+1,g.bottom-g.y+1
      local cx,cy=(g.x+g.right+1)/2,(g.y+g.bottom+1)/2
      for y=g.y,g.bottom do for x=g.x,g.right do
        local i=y*W+x+1
        if before[i]~=0 and not protected[i] then
          local nx,ny=(x+0.5-cx)/(w/2),(y+0.5-cy)/(h/2)
          if nx*nx+ny*ny<=1 then pixels[i]=3 elseif before[i]==3 then pixels[i]=1 end
        end
      end end
      nubs=nubs+1
    end
    -- Give both feet a clean little toe pad and a shared internal sole height.
    for _,g in ipairs(footParts) do
      local bottom=71
      for y=g.y,bottom do for x=g.x,g.right do
        local i=y*W+x+1
        if before[i]~=0 and not protected[i] then
          local inset=(y==g.y or y==bottom) and 1 or 0
          local inside=x>=g.x+inset and x<=g.right-inset
          if inside then pixels[i]=2 elseif before[i]==2 then pixels[i]=1 end
        end
      end end
      feet=feet+1
    end
    local targetLayer=number==1 and output.layers[1] or output:newLayer()
    targetLayer.name=layer.name
    local image=Image(W+PAD*2,H,ColorMode.INDEXED)
    for y=0,H-1 do for x=0,W-1 do
      local i=y*W+x+1
      if (pixels[i]==0)~=(before[i]==0) then maskErrors=maskErrors+1 end
      if protected[i] then assert(pixels[i]==1,'Face changed') end
      if pixels[i]~=before[i] then changes=changes+1 end
      image:drawPixel(x+PAD,y,pixels[i])
    end end
    local targetCel=targetLayer:cel(1)
    if targetCel then targetCel.image=image;targetCel.position=Point(0,0) else output:newCel(targetLayer,1,image,Point(0,0)) end
  end
  assert(maskErrors==0,'Outer silhouette changed')
  assert(protectedCount==45,'Expected 10 two-pixel-square eyes and five one-pixel mouths')
  assert(feet==10 and nubs==12)
  assert(output:saveAs(root..'wiper-flat-polished.aseprite'))
  assert(output:saveAs(root..'wiper-flat-polished-native.png'))
  local report=assert(io.open(root..'flat-polish-report.json','w'))
  report:write(string.format('{"asepriteVersion":"%s","width":256,"height":80,"layers":5,"opaqueColors":3,"paletteEntries":4,"noShading":true,"facePixelsPreserved":%d,"silhouetteErrors":%d,"trimmedExcessOutlinePixels":%d,"roundedRedTips":%d,"refinedFeet":%d,"changedInteriorPixels":%d,"addedHorizontalMargin":8,"alphaValues":[0,255]}',tostring(app.version),protectedCount,maskErrors,trimmed,nubs,feet,changes))
  report:close()
  output:close();input:close()
end,debug.traceback)
if not ok then local f=assert(io.open(root..'finish-polish-error.txt','w'));f:write(tostring(err));f:close() end
