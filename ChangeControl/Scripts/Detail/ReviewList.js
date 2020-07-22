$(() => {
    var optimized_edit_rv = [];

    $(".rv-edit").click(function (e) { 
        e.preventDefault();
        
    });
    
    $(".rv-approve").click(function (e) { 
        e.preventDefault();
        console.log("review",e);
        let rv_id = e.target.value;
        swal({
            title: "Approve Review", 
            buttons: [true,"Approve"],
            icon: "warning",
        }).then((res) => {
            if(res){
                $.post(ApproveReviewPath, {review_id:rv_id}, (data) => {
                    $.post(GeneratePath,{
                        'mode': data.mail,
                        'topic_code':topic_code,
                    }).fail((error) => {
                        console.err(error);
                        swal("Error", "Cannot send email to Requestor, Please try again", "error");
                        return;
                    });

                    if(data){
                        swal("Success", "Change Status Success", "success").then(location.reload());
                    }else{
                        swal("Error", "User Password Not Correct", "error");
                    }
                },"json");
            }
        });
    });

    $("form#edit_review_form").submit((e) => {
        e.preventDefault();
        console.log("edit submit");
        $('#loading').removeClass('hidden')
        SerializeEditReviewForm();

        $.post(UpdateReviewPath, () => {
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
            
            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                $("#ReviewSubmit").prop("disabled",true)
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