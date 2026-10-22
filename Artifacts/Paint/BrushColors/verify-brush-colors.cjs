const fs = require('fs');
const { PNG } = require('C:/Users/handa/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/pngjs');
const root = 'D:/Github/Engine/';
const assetDir = 'Assets/RYU/Sprite/Charcter/Paint/';
const originalColors = ['172,167,149', '119,125,116', '222,215,190'];
const colors = {
  Red: [[238,67,59],[164,52,51],[255,140,103]],
  Yellow: [[248,200,48],[194,144,35],[255,234,130]],
  Blue: [[59,148,224],[45,97,163],[140,211,247]],
  TipMask: [[205,205,205],[145,145,145],[255,255,255]]
};
const report = {size:[512,512], frames:3, files:[]};
for (let frame=1; frame<=3; frame++) {
  const number = String(frame).padStart(2,'0');
  const source = PNG.sync.read(fs.readFileSync(root+assetDir+'Brush_'+number+'.png'));
  for (const [variant, shades] of Object.entries(colors)) {
    const name = 'Brush_'+variant+'_'+number+'.png';
    const image = PNG.sync.read(fs.readFileSync(root+assetDir+'BrushColors/'+name));
    if(image.width!==512 || image.height!==512) throw Error('Wrong size: '+name);
    let bristlePixels=0;
    for(let y=0;y<512;y++) for(let x=0;x<512;x++) {
      const i=(y*512+x)*4;
      const shade=source.data[i+3]===255 ? originalColors.indexOf(Array.from(source.data.subarray(i,i+3)).join(',')) : -1;
      const expected=shade>=0 ? [...shades[shade],255] : variant==='TipMask' ? [0,0,0,0] : source.data.subarray(i,i+4);
      if(shade>=0) bristlePixels++;
      for(let c=0;c<4;c++) if(image.data[i+c]!==expected[c]) throw Error('Unexpected pixel: '+name+' at '+x+','+y);
      if(image.data[i+3]!==0 && image.data[i+3]!==255) throw Error('Partial alpha: '+name);
      const cell=((Math.floor(y/4)*4)*512+Math.floor(x/4)*4)*4;
      for(let c=0;c<4;c++) if(image.data[i+c]!==image.data[cell+c]) throw Error('Broken 4px grid: '+name);
    }
    report.files.push({name,bristlePixels,unexpectedChanges:0,partialAlpha:0,broken4pxBlocks:0});
  }
}
fs.writeFileSync(root+'Artifacts/Paint/BrushColors/pixel-verification.json',JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify(report));
