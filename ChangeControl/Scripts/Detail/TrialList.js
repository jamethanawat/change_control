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
        }).then((res) => {
            if(res){
                $.post("/detail/ApproveTrial", {trial_id:tr_id}, (data) => {
                    if(data){
                        swal("Success", "Change Status Success", "success").then(location.reload());
                    }else{
                        swal("Error", "User Password Not Correct", "error");
                    }
                },"json");
            }
        });
    });

    $("form#edit_trial_form").submit((e) => {
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

            promises.push($.post("/detail/UpdateTrial",{ desc: trial_form[0].value},() => {
                console.log('Inserted trial');
                files.forEach(element => {
                    var Data = new FormData();
                    Data.append("file",element.file);
                    Data.append("description",element.description);
                    promises.push($.ajax({
                        type: "POST",
                        url: "/detail/SubmitFileTrial",
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
            }).fail(() => {
                swal("Error", "Trial is not succes, Please contact admin", "error");
                $('#loading').addClass('hidden')
            }));
            
            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                InsertReviewStatus = false;
                $("#trial_submit").prop("disabled",true)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
    });

});