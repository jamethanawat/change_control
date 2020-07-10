﻿$(() => {

    $(".cf-edit").click(function (e) { 
        e.preventDefault();
        
    });
    
    $(".cf-approve").click(function (e) { 
        e.preventDefault();
        console.log("confirm",e);
        let cf_id = e.target.value;
        swal({
            title: "Approve Confirm", 
            buttons: [true,"Approve"],
            icon: "warning",
        }).then((res) => {
            if(res){
                $.post("/detail/ApproveConfirm", {confirm_id:cf_id}, (data) => {
                    if(data){
                        swal("Success", "Change Status Success", "success").then(location.reload());
                    }else{
                        swal("Error", "User Password Not Correct", "error");
                    }
                },"json");
            }
        });
    });

    $("form#edit_confirm_form").submit((e) => {
        e.preventDefault();
        $('#loading').removeClass('hidden')
            let confirm_form = $("form#edit_confirm_form").serializeArray();
            var promises = [];

            files = file_list_alt;
            console.log("files",files);
            
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            promises.push($.post("/detail/UpdateConfirm",{ desc: confirm_form[0].value},() => {
                console.log('Inserted confirm');
                files.forEach(element => {
                    var Data = new FormData();
                    Data.append("file",element.file);
                    Data.append("description",element.description);
                    promises.push($.ajax({
                        type: "POST",
                        url: "/detail/SubmitFileConfirm",
                        data: Data,
                        cache: false,
                        processData: false,
                        contentType: false,
                        success: function () {
                            console.log('confirm file uploaded');
                        },error: function() {
                            swal("Error", "Upload file not success", "error");
                        }
                    }));
                });
            }).fail(() => {
                swal("Error", "Confirm is not succes, Please contact admin", "error");
                $('#loading').addClass('hidden')
            }));
            
            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                InsertReviewStatus = false;
                $("#confirm_submit").prop("disabled",cfue)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
    });

});