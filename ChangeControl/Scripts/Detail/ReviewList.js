var rv_edit_submit;
var new_rv_edit = [];

$(() => {

    var rv_form = $('form#edit_review_form').serializeArray();
    rv_form.forEach(function(rv){
        new_rv_edit.push(rv.name.split("-")[1]);
    });

    $(".rv-approve").click(function (e) { 
        e.preventDefault();
        //console.log("review",e);
        let rv_id = e.target.value;
        swal({
            title: "Approve Issue", 
            buttons: [true,"Approve"],
            icon: "warning",
        }).then((apr) => {
            if (apr) {
                $('#loading').removeClass('hidden')
                $.post(CheckAllReviewBeforeApprovePath, { topic_code: topic_code, isInternal: isInternal}, (res) => {
                    if(res == "True"){
                        var promises = [];

                        $.post(ApproveReviewPath, {topic_code:topic_code, review_id:rv_id}, (result) => {
                            if(result){
                                if(result.mail != "" && result.mail != null){
                                promises.push(
                                    $.post(GenerateMailPath,{ 'mode': result.mail, 'topic_code':topic_code, 'dept': result.dept, }).fail((error) => {
                                        console.error(error);
                                        $('#loading').addClass('hidden')
                                        swal("Error", "Cannot send email to Requestor, Please try again", "error");
                                        return;
                                    })
                                );
                                }
                                promises.push(
                                    $.post(CheckApproveIPPPath, {topic_code:topic_code}, (res) => { 
                                    if(res.status == "success" && (res.data != null && res.data.length > 0)){
                                        promises.push(
                                            $.post(GenerateMailPath,{ 'mode': 'InformIPP', 'topic_code':topic_code, 'dept_arry': res.data, }).fail((error) => {
                                            console.error(error);
                                                console.error("err: ipp");
                                                $('#loading').addClass('hidden')
                                            swal("Error", "Cannot send email to IPP, Please try again", "error");
                                            return;
                                            })
                                        );
                                    }
                                })
                                );
                                Promise.all(promises).then(() => {
                                    $('#loading').addClass('hidden')
                                    swal("Success", "Change Status Success", "success").then(location.reload());
                                });
                            } else {
                                $('#loading').addClass('hidden')
                                swal("Error", "User Password Not Correct", "error");
                            }
                        },"json");
                    } else {
                        $('#loading').addClass('hidden')
                        swal("Error", "Review status has been changed , Refresh in 2 Second.", "error").then(setTimeout(() => { location.reload(); }, 1500));
                    }
                })
            }
        });
    });

    $("form#edit_review_form").submit((e) => {
        rv_edit_submit = true;
        let RadioNotValidate = checkRadioAndInput(new_rv_edit,rv_edit_submit);
        let InputNotValidate = checkInputRequired();
        if(RadioNotValidate || InputNotValidate){
            return false; 
        }
        $("#edit_review").modal("hide")
        e.preventDefault();
        console.log("edit submit");
        $('#loading').removeClass('hidden')
        var optimized_edit_rv = SerializeEditReviewForm();

        $.post(UpdateReviewPath, {topic_id:topic_id, topic_code:topic_code}, (result) => {
            var promises = [];
            files = file_list_alt;
            console.log("files",files);
            
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            promises.push(files.forEach(element => {
                var Data = new FormData();
                Data.append("file",element.file);
                Data.append("description",element.description);
                Data.append("code",topic_code);
                $.ajax({
                    type: "POST",
                    url: InsertFilePath,
                    data: Data,
                    cache: false,
                    processData: false,
                    contentType: false,
                    success: function () {
                    },
                    error: function() {
                        swal("Error", "Upload file not success", "error");
                    }
                })
            }));

            optimized_edit_rv.forEach(element => {
                promises.push(
                    $.post(InsertReviewItemPath, {
                        'status' : element.status,
                        'description' : element.desc,
                        'id' : element.id,
                    },(data) => {
                        console.log('Inserted item');
                    }).fail(() => {
                        alert('error handling here');
                    })
                )
            });

            promises.push(
                $.post(GenerateMailPath,{ 'mode': 'ReviewUpdate', 'topic_code':topic_code, 'dept':result.dept,'pos':'Approver' }).fail((error) => {
                    console.error(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                })
            );

            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                $("#ReviewSubmit").prop("disabled",true)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
        }).fail(() => {
            swal("Status has been updated","will refresh in 2 second","error").then(setTimeout(() => { location.reload(); }, 1500));
            $('#loading').addClass('hidden')
        });

    });

    $('form#edit_review_form').on('keyup change paste', 'input, select, textarea', (e) => {
        checkRadioAndInput(new_rv_edit,rv_edit_submit);
        /* --------------- and check is valid or not after submit once -------------- */
        if(rv_edit_submit) checkInputRequired();
    });
    checkRadioAndInput(new_rv_edit,rv_edit_submit);

});
        function SerializeEditReviewForm(){
            var f_edit_list = [];
            $("form#edit_review_form [data-id]:not(:disabled)").each(function (i,v) {
                let rv_id = $(this).data('id');
                if(f_edit_list[`${rv_id}`] == null) f_edit_list[`${rv_id}`] = {id:rv_id, status: null, desc: null}
                
                if($(this).data('type') == "status" && $(this)[0].checked == true){
                    f_edit_list[`${rv_id}`].status = this.value;
                }else if($(this).data('type') == "desc"){
                    f_edit_list[`${rv_id}`].desc = this.value;
                }
            });
            return f_edit_list.filter(Boolean);
        }