var tr_edit_submit;
$(() => {

    $(".tr-edit").click(function (e) { 
        e.preventDefault();
        
    });
    
    $(".tr-approve").click(function (e) { 
        e.preventDefault();
        console.log("trial",e);
        let tr_id = e.target.value;
        swal({
            title: "Approve Trial", 
            buttons: [true,"Approve"],
            icon: "warning",
        }).then((apr) => {
            if(apr){
                $.post(CheckAllTrialBeforeApprovePath, {topic_code:topic_code}, (res) => {
                    if(res == "True"){
                        $.post(ApproveTrialPath, {trial_id:tr_id}, (result) => {
                            if(result){
                                if(result.mail != ""){
                                    $.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':topic_code, 'dept': result.dept, }).fail((error) => {
                                        console.error(error);
                                        swal("Error", "Cannot send email to Requestor, Please try again", "error");
                                        return;
                                    });
                                }
                                swal("Success", "Change Status Success", "success").then(location.reload());
                            }else{
                                swal("Error", "User Password Not Correct", "error");
                            }
                        },"json");
                    }else{
                        swal("Error", "Trial status has been changed , Refresh in 2 Second.", "error").then(setTimeout(() => { location.reload(); }, 1500));
                    }
                })
            }
        });
    });

    $("form#edit_trial_form").submit((e) => {
        tr_edit_submit = true;
        let InputNotValidate = checkInputRequired();
        if(InputNotValidate){
            return false; 
        }
        $("#edit_trial").modal("hide")
        e.preventDefault();

        $('#loading').removeClass('hidden')
            let trial_form = $("form#edit_trial_form").serializeArray();
            var promises = [];

            files = file_list_alt;
            console.log("files",files);
            
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            promises.push($.post(UpdateTrialPath,{ desc: trial_form[0].value},(result) => {
                console.log('Inserted trial');
                files.forEach(element => {
                    var Data = new FormData();
                    Data.append("file",element.file);
                    Data.append("description",element.description);
                    promises.push($.ajax({
                        type: "POST",
                        url: SubmitFileTrialPath,
                        data: Data,
                        cache: false,
                        processData: false,
                        contentType: false,
                        success: function () {
                            console.log('trial file uploaded');
                        },error: function() {
                            swal("Error", "Upload file not success", "error");
                        }
                    }));
                    
                    
                });
                
                promises.push($.post(GenerateMailPath,{ 'mode': 'TrialUpdate', 'topic_code':topic_code, 'dept':result.dept,'pos':'Approver' }).fail((error) => {
                    console.error(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                }));
            }).fail(() => {
                swal("Status has been updated","will refresh in 2 second","error").then(setTimeout(() => { location.reload(); }, 1500));
                $('#loading').addClass('hidden')
            }));

            
            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                InsertReviewStatus = false;
                $("#trial_submit").prop("disabled",true)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
        
        $('form#edit_trial_form').on('keyup change paste', 'input, select, textarea', (e) => {
            /* --------------- and check is valid or not after submit once -------------- */
            if(tr_edit_submit) checkInputRequired();
        });
    });

});