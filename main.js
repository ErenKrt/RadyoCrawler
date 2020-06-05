const cheerio = require('cheerio');
const axios= require('axios');
const fs= require('fs');

var $ = null;

async function GetSoundServer(URL){
    var Data= (await axios({
        url:URL,
        type:"GET"
    })).data;

    var $$= cheerio.load(Data);

    var ID= $$("input[name=radyoid]").val();


    var PlayerData= (await axios({
        url:"https://www.canli-radyo.biz/wp-admin/admin-ajax.php?action=radyocal&radyo="+ID,
        type:"GET"
    })).data;

    $$=cheerio.load(PlayerData.html);

    return $$("source").attr("src");

}


async function CrawlList(){
    var RadyolarList= [];

    var Data= (await axios({
        url:"https://www.canli-radyo.biz",
        type:"GET"
    })).data;

    $= cheerio.load(Data);

    var Radyolar= $(".radio-list li");
    
    
    

    
    await new Promise((resolve) => { 
        
        Radyolar.each(async function(i, elem) {
            var Url= $(this).find("a").attr("href");

            
            var Resim= $(this).find("img").attr("src");
            var rgx= RegExp(/www.canli-radyo.biz\/wp-content\/uploads\/(.*?)\/(.*?)\/(.*)/);

            
            axios({
                method:"get",
                url:Resim,
                responseType: "stream"
            }).then(function(response){
                response.data.pipe(fs.createWriteStream("Resimler/"+rgx.exec(Resim)[3]));
            })

            var Isim= $(this).find("strong").text();
            var StreamUrl= await GetSoundServer(Url);

            RadyolarList.push({"name":Isim,"image":rgx.exec(Resim)[3],"link":StreamUrl});
            
            if(Radyolar.length==RadyolarList.length) resolve();

        })
        
    })
    

    
    fs.writeFile('radyolar.json', JSON.stringify(RadyolarList), 'utf8', function(){});

}

CrawlList();