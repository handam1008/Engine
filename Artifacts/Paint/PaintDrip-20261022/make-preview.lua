local dir="D:/Github/Engine/Artifacts/Paint/PaintDrip-20261022/"
local first=Image{fromFile=dir.."preview-frames/Frame_00.png"}
local sprite=Sprite(first.width,first.height,ColorMode.RGB)
sprite.layers[1].name="Paint Drip Preview: Red Yellow Blue"
for i=0,27 do
 if i>0 then sprite:newFrame() end
 local image=Image{fromFile=dir.."preview-frames/Frame_"..string.format("%02d",i)..".png"}
 local cel=sprite.layers[1]:cel(i+1)
 if cel then cel.image=image else sprite:newCel(sprite.layers[1],i+1,image,Point(0,0)) end
 sprite.frames[i+1].duration=i==27 and .3 or .04
end
sprite:saveAs(dir.."PaintDrip-preview.aseprite")
sprite:close()
