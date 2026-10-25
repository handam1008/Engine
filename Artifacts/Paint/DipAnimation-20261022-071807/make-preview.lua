local dir="D:/Github/Engine/Artifacts/Paint/DipAnimation-20261022-071807/"
local first=Image{fromFile=dir.."preview-frames/Frame_00.png"}
local sprite=Sprite(first.width,first.height,ColorMode.RGB)
sprite.layers[1].name="Unity pose preview: Red / Yellow / Blue"
for frame=0,33 do
 if frame>0 then sprite:newFrame() end
 local image=Image{fromFile=dir.."preview-frames/Frame_"..string.format("%02d",frame)..".png"}
 local cel=sprite.layers[1]:cel(frame+1)
 if cel then cel.image=image else sprite:newCel(sprite.layers[1],frame+1,image,Point(0,0)) end
 sprite.frames[frame+1].duration=frame==33 and .40 or .05
end
sprite:saveAs(dir.."Paint-Dip-Preview.aseprite")
sprite:close()
