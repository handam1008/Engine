const fs=require('fs'),{PNG}=require('C:/Users/handa/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/pngjs');
const dir='D:/Github/Engine/Artifacts/Paint/Unity/';
const sheet=PNG.sync.read(fs.readFileSync('D:/Github/Engine/Artifacts/Paint/Paint-Parts-v4.png'));
const report={strength:0.8464,durationsMs:[80,80,90],size:[512,512],maxDisplacementPixels:4,method:'Aseprite128 integerpixel inversewarp, approved12x12 localfields, affine removed,3phase; nearest4x',parts:{}};
for(const [partIndex,name]of ['Palette','Brush'].entries()){
 const base=PNG.sync.read(fs.readFileSync(dir+name+'_Static.png')),colors=new Set(),hashes=new Set(),frames=[];
 for(let y=0;y<512;y++)for(let x=0;x<512;x++){const a=(y*512+x)*4,b=(y*1024+x+partIndex*512)*4;for(let c=0;c<4;c++)if(base.data[a+c]!==sheet.data[b+c])throw Error('Static differs from approved sheet');if(base.data[a+3])colors.add(base.data[a]+','+base.data[a+1]+','+base.data[a+2]);}
 for(let frame=1;frame<=3;frame++){
  const p=PNG.sync.read(fs.readFileSync(dir+name+'_'+String(frame).padStart(2,'0')+'.png'));
  if(p.width!==512||p.height!==512)throw Error('Unexpected size');let changed=0,interiorChanged=0,partialAlpha=0,nonuniformBlocks=0;const mask=new Uint8Array(128*128);
  for(let y=0;y<512;y++)for(let x=0;x<512;x++){const i=(y*512+x)*4,a=p.data[i+3];if(a!==0&&a!==255)partialAlpha++;if(a&&!colors.has(p.data[i]+','+p.data[i+1]+','+p.data[i+2]))throw Error('Newcolor');if(a&&(x===0||x===511||y===0||y===511))throw Error('Clipped');if(!p.data.subarray(i,i+4).equals(base.data.subarray(i,i+4))){changed++;if(base.data[i+3]&&a)interiorChanged++;}}
  for(let y=0;y<512;y+=4)for(let x=0;x<512;x+=4){const i=(y*512+x)*4;for(let dy=0;dy<4;dy++)for(let dx=0;dx<4;dx++)for(let c=0;c<4;c++)if(p.data[((y+dy)*512+x+dx)*4+c]!==p.data[i+c])nonuniformBlocks++;mask[(y/4)*128+x/4]=p.data[i+3]?1:0;}
  const seen=new Uint8Array(mask.length),components=[];
  for(let s=0;s<mask.length;s++){if(!mask[s]||seen[s])continue;const q=[s];seen[s]=1;for(let n=0;n<q.length;n++){const i=q[n],x=i%128,y=Math.floor(i/128);for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){const nx=x+dx,ny=y+dy,j=ny*128+nx;if(nx>=0&&nx<128&&ny>=0&&ny<128&&mask[j]&&!seen[j]){seen[j]=1;q.push(j);}}}components.push(q.length);}
  if(partialAlpha||nonuniformBlocks||components.length!==1||!interiorChanged)throw Error('Invalid pixel grid,disconnected contour,or frozeninterior:'+name+frame+' '+JSON.stringify(components));
  const hash=require('crypto').createHash('sha256').update(p.data).digest('hex');hashes.add(hash);frames.push({frame,changedPixels:changed,interiorChangedPixels:interiorChanged,components:components.length,partialAlpha,nonuniform4pxBlocks:nonuniformBlocks,sha256:hash});
 }
 if(hashes.size!==3)throw Error('Repeatedframe');report.parts[name]={matchesApprovedStatic:true,newVisibleColors:0,frames};
}
fs.writeFileSync(dir+'wiggle-verification.json',JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report));
