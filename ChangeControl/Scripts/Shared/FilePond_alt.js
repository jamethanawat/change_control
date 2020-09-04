$(() => {
    FilePond.registerPlugin(FilePondPluginFileEncode);
    FilePond.registerPlugin(FilePondPluginFileValidateSize);
    const inputElement = document.querySelector('input[name="filepond_alt"]');
    var file_length = 0;
    var file_arry = [];
    var file_detail = [];
    var nd_bad_file = 0;
    var pond_alt = FilePond.create( inputElement,{
        maxFiles: 20,
        allowMultiple: true,
        required: false,
        labelIdle: `<i class="fas fa-paperclip"></i> Drag &amp; Drop your files or <span class="filepond--label-action"> Browse</span> (Limit 10MB)`,
        maxFileSize: '10MB'
    });

    var count = 0;
    pond_alt.on('addfile', (error, item) => {
        file_length = $(".filepond--file").length - $(".filepond--item[data-filepond-item-state='load-invalid'] > fieldset > .filepond--file").length;
        var file_obj = {id : null, detail : null,description : null};
        if(item.status == 8){
            nd_bad_file++;
            $(".btn-info").prop("disabled",true);
            swal("File too large!!", `${item.filename} is bigger than 10MB. \n Please delete and upload smaller file. \n\n *This file will not upload to server`, "error");
        }
        if (error) {
            console.log(error);
            return;
        }

        let fp_file = $(".filepond--item[data-filepond-item-state='idle'] > fieldset > .filepond--file");
            fp_file.eq(count).addClass("description");
            
            if(file_detail.length == 0){
                $(`#filepond--item-${item.id} .filepond--file`).append(''+
                // fp_file.eq(count).append(''+
                `<button class="filepond--file-action-button filepond--action filepond-add-desc" type="button" data-align="right" id="b${item.id}" style="transform: translate3d(0px, 0px, 0px) scale3d(1, 1, 1); opacity: 1;">`+
                `<i class="fas fa-pen" style="transform: scale(0.7);"></i>`+
                `</svg><span>Add description</span></button>`+
                `<div class="filepond--file-status" style="transform: translate3d(0px, 0px, 0px); opacity: 1;"><span class="filepond--file-status-main" id="s${item.id}" style=" display:none">&nbsp;</span><span class="filepond--file-status-sub">tap to edit</span></div>`+
                '');   
            }else{
                if(file_detail[count].id != null) file_obj.id = file_detail[count].id;
                if(file_detail[count].description != "" && file_detail[count].description != null){
                    file_obj.description = file_detail[count].description;
                $(`#filepond--item-${item.id} .filepond--file`).append(''+
                // fp_file.eq(count).append(''+
                    `<button class="filepond--file-action-button filepond--action filepond-add-desc" type="button" data-align="right" id="b${item.id}" style="transform: translate3d(0px, 0px, 0px) scale3d(1, 1, 1); opacity: 1;">`+
                    `<i class="fas fa-pen" style="transform: scale(0.7);"></i>`+
                    `</svg><span>Add description</span></button>`+
                    `<div class="filepond--file-status" style="transform: translate3d(0px, 0px, 0px); opacity: 1;"><span class="filepond--file-status-main" id="s${item.id}">&nbsp;${file_detail[count].description}</span><span class="filepond--file-status-sub">tap to edit</span></div>`+
                    '');
                }
            }
                
        file_obj.detail = item;
        file_list_alt.push(file_obj);
        
        // count++;
        // if(file_list_alt.length == file_length){
            $(".filepond-add-desc").off("click");
            $(".filepond-add-desc").on("click",(e) => {
                add_description(e.target.id);
            })
        //     count = 0;
        //     origin_file = file_arry;
        //     file_detail = [];
        //     file_arry = [];
        // }
        console.log(file_list_alt);
    });

    pond_alt.on('removefile', (error,item) =>{
        if(item.status == 8){
            nd_bad_file--;
            if(nd_bad_file == 0) $(".btn-info").prop("disabled",false);
        }
        if (error) {
            console.log('Oh no');
            return;
        }

        file_list_alt = file_list_alt.filter(e => e.detail.id != item.id);
    });

    function add_description(update_id){
        $(".modal").not("#resubmit_modal").modal("hide")
        update_id = update_id.slice(1);
        update_i = file_list_alt.findIndex(item => item.detail.id == update_id);
        swal({
            title: "Please provide a description.", 
            text: "โปรดระบุคำอธิบายของไฟล์หรือเอกสารดังกล่าว", 
            content: {
                element: "input",
                attributes: {
                    placeholder: "คำอธิบาย",
                    value: file_list_alt[update_i].description,
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
            $(".modal").not("#resubmit_modal").modal("show");
            if(result != null && result != "" && result != "clear"){
                console.log(`${update_id}`);
                $(`.filepond--file-status-main#s${update_id}`).html(result).show("slow");
                file_list_alt[update_i].description = result;
                swal({
                    title: "สำเร็จ", 
                    text: "เพิ่มคำอธิบายสำเร็จ", 
                    icon:"success",
                });
            }else if(result != null){
                $(`.filepond--file-status-main#s${update_id}`).hide("slow");
                file_list_alt[update_i].description = null;
            }
        });
    }


});
