var InsertReviewStatus = false;
var optimized_arr = [];
var ReviewStatus = false;
var DepartmentLists;
var file_list = [];
var file_list_alt = [];
var isQC;
var isTrialable;
var isReview;
var resubmit_formIsEmpty = true;
var TrialIsEmpty = true;

$(() => {
    
    if(topic_status == "Request" || !isQC || !isReview){
        $(".zoom-fab#change_status").addClass("hide-fab");
    }

/* -------------------------------- Go to top ------------------------------- */
    $('[data-toggle="tooltip"]').tooltip();
    var btn = $('#Top');
    $(window).scroll(() => {
        if ($(window).scrollTop() > 600) {
            btn.addClass('show');
        } else {
            btn.removeClass('show');
        }
    });

    btn.on('click', function (e) {
        e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, '300');
    });

/* --------------------------- Resubmit's Validate -------------------------- */
var rsm_validator = $('#resubmit_form').validate({
    rules: {
        desc: {required: true,},
        due_date: {required: true,date: true}
    },
    messages: {
        desc: {required: "Please enter a description",},
        due_date: {required: "Please enter a due date",date: "Please enter a valid date"}
    },
    errorElement: 'span',
    errorPlacement: function (error, element) {
      error.addClass('invalid-feedback');
      element.closest(".col-form-label").append(error);
      $(".btn-success").prop('disabled', true);
    },
    highlight: function (element, errorClass, validClass) {$(element).addClass('is-invalid');},
    unhighlight: function (element, errorClass, validClass) {$(element).removeClass('is-invalid');}
});
/* ------------------------- Resubmit's Wizard modal ------------------------ */
    $("#resubmit_modal").modalWizard();
    $("#resubmit_modal").on("navigate", (e, navDir, stepNumber) => {
        if($("#resubmit_modal").attr("data-current-step") == 3){
            let quick_form = $("form#resubmit_form").serializeArray();
            console.log(quick_form);

            let related = due_date = desc = "";
            
            quick_form.forEach(item => {
                if(item.value == "1"){
                    if(related == ""){
                        related = item.name;
                    }else{
                        related = related + " , " + item.name;
                    }
                }else if(item.name == "desc"){
                    desc = item.value;
                }else if(item.name == "due_date"){
                    due_date = item.value;
                }
            });

            moment.locale("th");
            due_date = moment(due_date,"DD-MM-YYYY").format('LL');
            $("#related_date").val(due_date);
            $("#related_dept").html(related);
            $("#related_desc").html(desc);
            console.log("related: ",related);
        }else{
            $(".btn-success").prop('disabled', true);
            if(navDir == "prev"){
                relatedValidate();
            }else{
                if(!resubmit_formIsEmpty){
                    $(".btn-success").prop('disabled', (rsm_validator.form()) ? false : true); 
                }
            }
        }
    });
    
/* ---------------------------- Resubmit's Validate ---------------------------- */
    $("#resubmit_form [name='desc'], #resubmit_form [name='due_date']").on("keydown keyup click change", () => {
        console.log("triggered");
        if($("#resubmit_form [name='desc']").val() != "" && $("#resubmit_form [name='due_date']").val() !== ""){
            resubmit_formIsEmpty = false;
        }else{
            resubmit_formIsEmpty = true;
        }
        if(!resubmit_formIsEmpty){
            $(".btn-success").prop('disabled', (rsm_validator.form()) ? false : true); 
        }
    });

/* ---------------------------- Trial's Validate ---------------------------- */
    var tr_validator = $('#Trial').validate({
        rules: { 
          tr_desc: {required: true,},
        },
        messages: {
          tr_desc: {required: "Please enter a description",},
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
          error.addClass('invalid-feedback');
          element.closest(".col-form-label").append(error);
          $(".btn-success").prop('disabled', true);
        },
        highlight: function (element, errorClass, validClass) {$(element).addClass('is-invalid');},
        unhighlight: function (element, errorClass, validClass) {$(element).removeClass('is-invalid');}
    });
    
    $("#Trial [name='tr_desc']").on("keydown keyup", () => {
        $("#tr_submit").prop('disabled', (tr_validator.form()) ? false : true); 
    });

    var cf_validator = $('#Confirm').validate({
        rules: { 
          cf_desc: {required: true,},
        },
        messages: {
          cf_desc: {required: "Please enter a description",},
        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
          error.addClass('invalid-feedback');
          element.closest(".col-form-label").append(error);
          $(".btn-success").prop('disabled', true);
        },
        highlight: function (element, errorClass, validClass) {$(element).addClass('is-invalid');},
        unhighlight: function (element, errorClass, validClass) {$(element).removeClass('is-invalid');}
    });
    
    $("#Confirm [name='cf_desc']").on("keydown keyup", () => {
        $("#cf_submit").prop('disabled', (cf_validator.form()) ? false : true); 
    });
    
/* --------------------------- Department checkbox -------------------------- */
    $.each(DepartmentLists, (key,val) => {
        if($(`.${val.Name}`).length == $(`.${val.Name}:checked`).length){
            $(`#${val.Name}`).prop('checked', true);
            relatedValidate();
        }
        // console.log(DepartmentLists);
        $(`#${val.Name}`).change(function(e) {
                $(`.${val.Name}`).each(function() {
                    this.checked = (e.target.checked) ? true : false;
                });
            relatedValidate();
        });
    
        $(`.${val.Name}`).click(function () {
            if ($(this).is(":checked")) {
                var isAllChecked = 0;
                $(`.${val.Name}`).each(() => { if(!this.checked) isAllChecked = 1; });
                if(isAllChecked == 0) $(`#${val.Name}`).prop("checked", true);     
            }else{
                $(`#${val.Name}`).prop("checked", false);
            }
            relatedValidate();
        });
    });

    function relatedValidate(){
        checkbox_dept = $('input:checkbox.qForm.checkSingle:checked').length
        $(".btn-success").prop('disabled', (checkbox_dept > 0) ? false : true);
    }

    $('[data-toggle="datepicker"]').datepicker({
        format: 'dd-mm-yyyy'
    });
    
   
/* ------------------------------ Submit review topic ------------------------------ */
    $("form#review").submit((e) => {
        e.preventDefault();
        $('#loading').removeClass('hidden')
        SerializeReviewForm();

        $.post(SubmitReviewPath, (result) => {
            if(result.mail != ""){
                $.post(GeneratePath,{
                    'mode': result.mail,
                    'topic_code':topic_code,
                    'dept':result.dept,
                }).fail((error) => {
                    console.err(error);
                    swal("Error", "Cannot send email to Requestor, Please try again", "error");
                    return;
                })
            }
            var promises = [];
            files = file_list;
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
                    error: function() {
                        swal("Error", "Upload file not success", "error");
                    }
                }));
            });

            optimized_arr.forEach(element => {
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
/* --------------------------- Submit trial topic --------------------------- */
$("form#Trial").submit((e) => {
    e.preventDefault();
    $('#loading').removeClass('hidden')
        let trial_form = $("form#Trial").serializeArray();
        var promises = [];

        files = file_list;
        console.log("files",files);
        
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }

        promises.push($.post(SubmitTrialPath,{ desc: trial_form[0].value},() => {
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

/* -------------------------- Submit confirm topic -------------------------- */
$("form#Confirm").submit((e) => {
    e.preventDefault();
    $('#loading').removeClass('hidden')
        let confirm_form = $("form#Confirm").serializeArray();
        var promises = [];

        files = file_list;
        console.log("files",files);
        
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }

        promises.push($.post(SubmitConfirmPath,{ desc: confirm_form[0].value},() => {
            console.log('Inserted trial');
            files.forEach(element => {
                var Data = new FormData();
                Data.append("file",element.file);
                Data.append("description",element.description);
                promises.push($.ajax({
                    type: "POST",
                    url: SubmitFileConfirmPath,
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
            $("#cf_submit").prop("disabled",true)
            swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
        })
});
/* ------------------------------ Apply resubmit ----------------------------- */
    $("form#resubmit_form").submit((e) => {
        e.preventDefault();
        let quick_form = $("form#resubmit_form").serialize();
        console.log(quick_form);
        $.post(SubmitRelatedPath, quick_form, () =>{
            console.log('Related created');
            $.post(RequestResubmitPath, quick_form, (res) =>{
                console.log('Resubmit created');
                if(res){
                    swal("Success", "Resubmit Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
                }else{
                    swal("Error", "Resubmit Not Success, Please Try Again", "error");
                }
            });
        });
    });

/* ---------------------------- Response resubmit --------------------------- */
    $("#submit_reply_form").click((e) => {
        e.preventDefault();
        $('#loading').removeClass('hidden')
            let form_response = $("form.reply_form").serializeArray();
            var response_id = $("form.reply_form").attr("id");
            var promises = [];

            files = file_list_alt;
            console.log("files",files);
            
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            promises.push($.post(SubmitResponsePath,{ desc: form_response[0].value, resubmit_id: response_id },() => {
                console.log('Inserted item');
                files.forEach(element => {
                    var Data = new FormData();
                    Data.append("file",element.file);
                    Data.append("description",element.description);
                    promises.push($.ajax({
                        type: "POST",
                        url: SubmitFileResponsePath,
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
            }).fail(() => {
                swal("Error", "Reply not success", "error");
            }));

            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                InsertReviewStatus = false;
                $("#ResponseSubmit").prop("disabled",true)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
    });

/* ----------- Identify and seperate beween description and radio ----------- */
    function SerializeReviewForm(){
        var datastring = $("form#review").serializeArray();
        console.log(datastring);
        for(var i=0 ; i<datastring.length ; i++){
            
            if(datastring[i].name != "filepond"){
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
                    console.error("optimized_arr error");                
                }else{
                    this.optimized_arr.push(new_item);
                }
            }
        }
        console.log("optimized",optimized_arr);
    }

/* --------------------------- Change topic status -------------------------- */
    $(".zoom-fab#change_status").click(() => {
        var change_status = "Change status from";
        var new_status = 0;
        if(topic_status == "Review"){
            change_status += " Review to Trial";
            new_status = 9;
        }else if(topic_status == "Trial"){
            change_status += " Trial to Confirm";
            new_status = 10;
        }else if(topic_status == "Confirm"){
            change_status += " Confirm to Close";
            new_status = 11;
        }
        swal({
            title: "Change Status", 
            buttons: [true,"Confirm"],
            text: change_status, 
            icon:"warning",
        }).then((isChanged) => {
            if(isChanged){
                $.post(UpdateTopicStatusPath, {topic_code:topic_code,status:new_status}, (data) => {
                    if(data){
                        swal("Success", "Change Status Success", "success").then(location.reload());
                    }else{
                        swal("Error", "User Password Not Correct", "error");
                    }
                },"json");
            }
        });
    });

/* ---- Clear file in filepond when changing from Resubmit topic to topic --- */
    var reply_id = 0;
    $(".reply_modal").click((e) => {
        let new_reply_id = e.target.id.split("-")[1];
        if(new_reply_id != reply_id){
            console.log("id not matched");
            $(".reply_form textarea").val('');
            reply_id = new_reply_id;
            if(file_list_alt.length > 0){
                file_list_alt.forEach(e => {
                    alt_removeFile(e.detail.id);
                });
                file_list_alt = [];
            }
            $("form.reply_form").attr("id", new_reply_id);
        }else{
            console.log("id matched");
        }
    });

});

