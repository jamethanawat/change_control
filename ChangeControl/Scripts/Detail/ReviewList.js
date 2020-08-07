$(() => {
    var optimized_edit_rv = [];

    $(".rv-approve").click(function (e) { 
        e.preventDefault();
        console.log("review",e);
        let rv_id = e.target.value;
        swal({
            title: "Approve Review", 
            buttons: [true,"Approve"],
            icon: "warning",
        }).then((apr) => {
            if(apr){
                $.post(CheckAllReviewBeforeApprovePath, {topic_code:topic_code}, (res) => {
                    if(res == "True"){
                        $.post(ApproveReviewPath, {review_id:rv_id}, (result) => {
                            if(result){
                                if(result.mail != ""){
                                    $.post(GenerateMailPath,{
                                        'mode': result.mail,
                                        'topic_code':topic_code,
                                        'dept': result.dept,
                                    }).fail((error) => {
                                        console.error(error);
                                        swal("Error", "Cannot send email to Requestor, Please try again", "error");
                                        return;
                                    });
                                }
                                $.post(CheckApproveIPPPath, {topic_code:topic_code}, (res) => {
                                    if(res.status == "success"){
                                        $.post(GenerateMailPath,{
                                            'mode': 'InformIPP',
                                            'topic_code':topic_code,
                                            'dept_arry': res.data,
                                        }).fail((error) => {
                                            console.error(error);
                                            console.error("err: ipp");
                                            swal("Error", "Cannot send email to IPP, Please try again", "error");
                                            return;
                                        });
                                    }
                                })
                                swal("Success", "Change Status Success", "success").then(location.reload());
                            }else{
                                swal("Error", "User Password Not Correct", "error");
                            }
                        },"json");
                    }else{
                        swal("Error", "Review status has been changed , Refresh in 2 Second.", "error").then(setTimeout(() => { location.reload(); }, 1500));
                    }
                })
            }
        });
    });

    $("form#edit_review_form").submit((e) => {
        e.preventDefault();
        console.log("edit submit");
        $('#loading').removeClass('hidden')
        SerializeEditReviewForm();

        $.post(UpdateReviewPath, (result) => {
            var promises = [];
            files = file_list_alt;
            console.log("files",files);
            
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            files.forEach(element => {
                var Data = new FormData();
                Data.append("file",element.file);
                Data.append("description",element.description);
                promises.push($.ajax({
                    type: "POST",
                    url: SubmitFilePath,
                    data: Data,
                    cache: false,
                    processData: false,
                    contentType: false,
                    success: function () {
                    },
                    error: function() {
                        swal("Error", "Upload file not success", "error");
                    }
                }));
            });

            optimized_edit_rv.forEach(element => {
                promises.push(
                    $.post(SubmitReviewItemPath, {
                        'status' : element.status,
                        'description' : element.description,
                        'id' : element.id,
                    },(data) => {
                        console.log('Inserted item');
                    }).fail(() => {
                        alert('error handling here');
                    })
                )
            });

            promises.push($.post(GenerateMailPath,{
                    'mode': 'ReviewUpdate',
                    'topic_code':topic_code,
                    'dept':result.dept,
                }).fail((error) => {
                    console.error(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                })
            );

            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                $("#ReviewSubmit").prop("disabled",true)
                $("#edit_review").modal("hide")
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
        });

    });

    function SerializeEditReviewForm(){
        var datastring = $("form#edit_review_form").serializeArray();
        console.log(datastring);
        for(var i=0 ; i<datastring.length ; i++){
            
            if(datastring[i].name != "filepond_alt"){
                let new_item = {};
                new_item.id = 999;
                new_item.status = null;
                new_item.description = null;
                
                if(!datastring[i].hasOwnProperty('splitted_value')){
                    datastring[i].splitted_value = datastring[i].name.split('-');
                }
                if(i+1 != datastring.length){
                    if(!datastring[i+1].hasOwnProperty('splitted_value')){
                        datastring[i+1].splitted_value = datastring[i+1].name.split('-'); //split desc-1234 to [0]desc, [1]1234
                    }
                    if(datastring[i].splitted_value[0] == "rd"){
                        new_item.id = datastring[i].splitted_value[1];
                        new_item.status = datastring[i].value; // rd -> radio equal status
                        if(datastring[i+1].splitted_value[0] == "desc" && datastring[i].splitted_value[1] == datastring[i+1].splitted_value[1]){
                            new_item.description = datastring[i+1].value;
                        }
                    }else if(datastring[i].splitted_value[0] == "desc"){
                        new_item.id = datastring[i].splitted_value[1];
                        new_item.description = datastring[i].value; // desc -> description
                        if(datastring[i+1].splitted_value[0] == "rd" && datastring[i].splitted_value[1] == datastring[i+1].splitted_value[1]){
                            new_item.status = datastring[i+1].value;
                        }
                    }
                    if(datastring[i].splitted_value[1] == datastring[i+1].splitted_value[1]){
                        i++;
                    }
                }else{
                    new_item.id = datastring[i].splitted_value[1];
                    if(datastring[i].splitted_value[0] == "rd"){
                        new_item.status = datastring[i].value;
                    }else if(datastring[i].splitted_value[0] == "desc"){
                        new_item.description = datastring[i].value;
                    }
                }
                if(new_item.id == 999 && new_item.status == null && new_item.description == null){ // exception case
                    console.error("optimized_edit_rv error");                
                }else{
                    optimized_edit_rv.push(new_item);
                }
            }
        }
        console.log("optimized_edit_rv",optimized_edit_rv);
    }
});