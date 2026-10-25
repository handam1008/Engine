local dir="D:/Github/Engine/Artifacts/Paint/PaintHit-20261022/"
local first=Image{fromFile=dir.."preview-frames/Frame_00.png"}
local sprite=Sprite(first.width,first.height,ColorMode.RGB)
sprite.layers[1].name="Paint Hit Preview: Red Yellow Blue"
for i=0,15 do
 if i>0 then sprite:newFrame() end
 local image=Image{fromFile=dir.."preview-frames/Frame_"..string.format("%02d",i)..".png"}
 local cel=sprite.layers[1]:cel(i+1)
 if cel then cel.image=image else sprite:newCel(sprite.layers[1],i+1,image,Point(0,0)) end
 sprite.frames[i+1].duration=i==15 and .35 or .035
end
sprite:saveAs(dir.."PaintHit-preview.aseprite")
sprite:close()
