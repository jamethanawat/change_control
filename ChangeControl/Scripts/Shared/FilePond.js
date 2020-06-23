$(() => {
    FilePond.registerPlugin(FilePondPluginFileEncode);
    const inputElement = document.querySelector('input[name="filepond"]');
    var file_length = 0;
    var file_arry = [];
    var file_detail = [];
    var origin_file = [];
    var pond = FilePond.create( inputElement,{
        maxFiles: 20,
        allowMultiple: true,
        required: false,
        labelIdle: `<i class="fas fa-paperclip"></i> Drag &amp; Drop your files or <span class="filepond--label-action"> Browse  </span>`
    });
    // pond.addFile('/nature.jpg');


    pond.on('addfilestart',() => { 
        file_length = $('.filepond--item').length;
    });

    var count = 0;
    pond.on('addfile', (error, item) => {
        var file_obj = {id : null, detail : null,description : null};
        console.log("file length :",file_length);        
        if (error) {
            console.log(error);
            console.log('Oh no');
            return;
        }

        let fp_file = $(".filepond--file");
            fp_file.eq(count).addClass("description");
            
            if(file_detail.length == 0){
                fp_file.eq(count).append(''+
                `<button class="filepond--file-action-button filepond--action filepond-add-desc" type="button" data-align="right" id="b${item.id}" style="transform: translate3d(0px, 0px, 0px) scale3d(1, 1, 1); opacity: 1;">`+
                `<i class="fas fa-pen" style="transform: scale(0.7);"></i>`+
                `</svg><span>Add description</span></button>`+
                `<div class="filepond--file-status" style="transform: translate3d(0px, 0px, 0px); opacity: 1;"><span class="filepond--file-status-main" id="s${item.id}" style=" display:none">&nbsp;</span><span class="filepond--file-status-sub">tap to edit</span></div>`+
                '');   
            }else{
                if(file_detail[count].id != null) file_obj.id = file_detail[count].id;
                if(file_detail[count].description != "" && file_detail[count].description != null){
                    file_obj.description = file_detail[count].description;
                    fp_file.eq(count).append(''+
                    `<button class="filepond--file-action-button filepond--action filepond-add-desc" type="button" data-align="right" id="b${item.id}" style="transform: translate3d(0px, 0px, 0px) scale3d(1, 1, 1); opacity: 1;">`+
                    `<i class="fas fa-pen" style="transform: scale(0.7);"></i>`+
                    `</svg><span>Add description</span></button>`+
                    `<div class="filepond--file-status" style="transform: translate3d(0px, 0px, 0px); opacity: 1;"><span class="filepond--file-status-main" id="s${item.id}">&nbsp;${file_detail[count].description}</span><span class="filepond--file-status-sub">tap to edit</span></div>`+
                    '');
                }
            }
                
        file_obj.detail = item;
        file_list.push(file_obj);
        
        count++;
        console.log("compared: ",file_list.length);
        if(file_list.length == file_length){
            $(".filepond-add-desc").off("click");
            $(".filepond-add-desc").on("click",(e) => {
                add_description(e.target.id);
            })
            count = 0;
            origin_file = file_arry;
            file_detail = [];
            file_arry = [];
        }
        console.log(file_list);
    });

    pond.on('removefile', (error,file) =>{
        if (error) {
            console.log('Oh no');
            return;
        }

        file_list = file_list.filter(item => item.detail.id != file.id);
    });

    function add_description(update_id){
        update_id = update_id.slice(1);
        console.log("before: ",file_list);
        update_i = file_list.findIndex(item => item.detail.id == update_id);
        swal({
            title: "Please provide a description.", 
            text: "โปรดระบุคำอธิบายของไฟล์หรือเอกสารดังกล่าว", 
            content: {
                element: "input",
                attributes: {
                    placeholder: "คำอธิบาย",
                    value: file_list[update_i].description,
                },
               
            },
            buttons:{
                clear: {
                    text: "Clear",
                    value: "clear",
                },
                confirm: {
                    text: "Confirm",
                    value: true,
                }
            },
            icon:"warning",
        }).then((result) => {
            if(result != null && result != "" && result != "clear"){
                console.log(`${update_id}`);
                $(`.filepond--file-status-main#s${update_id}`).html(result).show("slow");
                file_list[update_i].description = result;
                swal({
                    title: "สำเร็จ", 
                    text: "เพิ่มคำอธิบายสำเร็จ", 
                    icon:"success",
                });
            }else if(result == "clear"){
                $(`.filepond--file-status-main#s${update_id}`).hide("slow");
                file_list[update_i].description = null;
                swal({
                    title: "สำเร็จ", 
                    text: "เพิ่มคำอธิบายสำเร็จ", 
                    icon:"success",
                });
            }
        });
    }

    window.pondAddFile = (path = "/nature.jpg") => {
        pond.addFile(path, {options: {
            file: {
                name: 'my-file.png',
            }
        }});
    };

    window.addPondFile = () => {

    }

    window.addFile = (id = null,file,description = null) => {
        file_arry.push(file);
        let detail = {
            id: id,
            description: description
        }
        if(description != null) file_detail.push(detail);
        file_length++;
    }

    window.getFile = () => {
        return file_arry;
    }

    window.pondSetOptions = () => {
        pond.setOptions({
            files:file_arry
        });
    }

});
