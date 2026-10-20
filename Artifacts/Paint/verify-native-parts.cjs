const fs=require('fs');
const {PNG}=require('C:/Users/handa/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/pngjs');
const base=process.argv[2];
if(!base)throw Error('Provide the native export basename.');
const p=PNG.sync.read(fs.readFileSync(base+'.png')),s=PNG.sync.read(fs.readFileSync(base+'-logical.png'));
let partialAlpha=0,dirtyTransparentPixels=0,nonuniform8pxBlocks=0,exportMismatch=0;
const colors=new Set();
for(let y=0;y<p.height;y++)for(let x=0;x<p.width;x++){
  const i=(y*p.width+x)*4,a=p.data[i+3];if(a!==0&&a!==255)partialAlpha++;
  if(a)colors.add(p.data[i]+','+p.data[i+1]+','+p.data[i+2]);else if(p.data[i]||p.data[i+1]||p.data[i+2])dirtyTransparentPixels++;
  const j=(Math.floor(y/4)*s.width+Math.floor(x/4))*4;for(let c=0;c<4;c++)if(p.data[i+c]!==s.data[j+c])exportMismatch++;
}
for(let y=0;y<p.height;y+=8)for(let x=0;x<p.width;x+=8){const i=(y*p.width+x)*4;let same=true;for(let dy=0;dy<8;dy++)for(let dx=0;dx<8;dx++)for(let c=0;c<4;c++)if(p.data[((y+dy)*p.width+x+dx)*4+c]!==p.data[i+c])same=false;if(!same)nonuniform8pxBlocks++;}
const mask=new Uint8Array(s.width*s.height),seen=new Uint8Array(mask.length);for(let i=0;i<mask.length;i++)mask[i]=s.data[i*4+3]?1:0;
const solidComponents=[],holes=[];
for(let start=0;start<mask.length;start++){
  if(seen[start])continue;const q=[start],value=mask[start];seen[start]=1;let edge=false;
  for(let a=0;a<q.length;a++){const i=q[a],x=i%s.width,y=Math.floor(i/s.width);if(!x||!y||x===s.width-1||y===s.height-1)edge=true;
    for(const [nx,ny]of[[x-1,y],[x+1,y],[x,y-1],[x,y+1]])if(nx>=0&&nx<s.width&&ny>=0&&ny<s.height){const j=ny*s.width+nx;if(!seen[j]&&mask[j]===value){seen[j]=1;q.push(j);}}}
  if(value)solidComponents.push(q.length);else if(!edge)holes.push(q.length);
}
if(p.width!==1024||p.height!==512||s.width!==256||s.height!==128||partialAlpha||dirtyTransparentPixels||nonuniform8pxBlocks||exportMismatch||solidComponents.length!==2||holes.length!==1)throw Error('Native export verification failed');
const report={tool:fs.readFileSync(base+'-aseprite-run.txt','utf8').split('\n')[0].trim(),nativeFile:base+'.aseprite',nativeSize:[s.width,s.height],outputSize:[p.width,p.height],cells:[{name:'Palette',rect:[0,0,512,512]},{name:'Brush',rect:[512,0,512,512]}],opaqueColors:colors.size,paletteIncludingTransparency:colors.size+1,partialAlpha,dirtyTransparentPixels,nonuniform8pxBlocks,exportMismatch,solidComponents,transparentThumbholes:holes.length,thumbholeNativePixels:holes[0],construction:'integer pixel coordinate polygons drawn inAsepriteLua; fixedsolidcolors; noimagegeneratorbitmapresampling',primaryNativeClusterSize:[2,2],exportScale:4,unityImported:false,animationCreated:false};
fs.writeFileSync(base+'.verification.json',JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report));
