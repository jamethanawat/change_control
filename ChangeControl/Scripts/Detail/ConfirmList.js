var cf_edit_submit;
$(() => {

    $(".cf-edit").click(function (e) { 
        e.preventDefault();
        
    });
    
    $(".cf-approve").click(function (e) { 
        e.preventDefault();
        //console.log("confirm",e);
        let cf_id = e.target.value;
        swal({
            title: "Approve Confirm", 
            buttons: [true,"Approve"],
            icon: "warning",
        }).then((apr) => {
            if (apr) {
                $('#loading').removeClass('hidden')
                $.post(CheckAllConfirmBeforeApprovePath, {topic_code:topic_code}, (res) => {
                    if(res == "True"){
                        $.post(ApproveConfirmPath, { topic_code: topic_code, confirm_id: cf_id, status: topic_status}, (result) => {
                            if(result){
                                if(result.mail != "" && result.mail != null){
                                    $.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':topic_code, 'dept': result.dept, }).fail((error) => {
                                        console.error(error);
                                        $('#loading').addClass('hidden')
                                        swal("Error", "Cannot send email to Requestor, Please try again", "error");
                                        return;
                                    });
                                }
                                $('#loading').addClass('hidden')
                                swal("Success", "Change Status Success", "success").then(location.reload());
                            } else {
                                $('#loading').addClass('hidden')
                                swal("Error", "User Password Not Correct", "error");
                            }
                        },"json");
                    } else {
                        $('#loading').addClass('hidden')
                        swal("Error", "Confirm status has been changed , Refresh in 2 Second.", "error").then(setTimeout(() => { location.reload(); }, 1500));
                    }
                })
            }
        });
    });

    $("form#edit_confirm_form").submit((e) => {
        cf_edit_submit = true;
        let InputNotValidate = checkInputRequired();
        if(InputNotValidate){
            return false; 
        }
        $("#edit_confirm").modal("hide")
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

            $.post(UpdateConfirmPath,{topic_id:topic_id, topic_code:topic_code, desc: confirm_form[0].value},(result) => {
                promises.push( files.forEach(element => {
                    var Data = new FormData();
                    Data.append("file",element.file);
                    Data.append("description",element.description);
                    Data.append("code",topic_code);
                    $.ajax({ 
                        type: "POST",
                        url: InsertFileConfirmPath,
                        data: Data,
                        cache: false,
                        processData: false,
                        contentType: false,
                        success: function () {
                            console.log('confirm file uploaded');
                        },error: function() {
                            swal("Error", "Upload file not success", "error");
                        }
                    });
                }));

                promises.push($.post(GenerateMailPath,{ 'mode': 'ConfirmUpdate', 'topic_code':topic_code, 'dept':result.dept,'pos':'Approver' }).fail((error) => {
                    console.error(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                }));

                Promise.all(promises).then(() => {
                    $('#loading').addClass('hidden')
                    
                    $("#confirm_submit").prop("disabled",true)
                    $("#edit_confirm").modal("hide")
                    swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
                })

            }).fail(() => {
                swal("Status has been updated","will refresh in 2 second","error").then(setTimeout(() => { location.reload(); }, 1500));
                $('#loading').addClass('hidden')
            });

            
            

        $('form#edit_confirm_form').on('keyup change paste', 'input, select, textarea', (e) => {
            /* --------------- and check is valid or not after submit once -------------- */
            if(cf_edit_submit) checkInputRequired();
        });
    });

});