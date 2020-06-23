var UpdateFileDescPath;
var InsertReviewStatus = false;
var optimized_arr = [];
var ReviewStatus = false;
var InsertFilePath;
var InsertFileResponsePath;
var InsertRelatedPath;
var InsertResponsePath;
var ReqResubmitPath;
var DepartmentLists;
var file_list = [];
var file_list_alt = [];
var isQC;
var isTrialable;
var quickFormIsEmpty = true;

$(() => {
    $("#quickForm [name='desc'], #quickForm [name='due_date']").on("keypress", () => {
        if($("#quickForm [name='desc']").val() != "" && $("#quickForm [name='due_date']").val() !== ""){
            quickFormIsEmpty = false;
        }else{
            quickFormIsEmpty = true;
        }
    });

    if(topic_status == "Request" || !isQC){
        $(".zoom-fab#change_status").addClass("hide-fab");
    }

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

    $("#resubmit_modal").modalWizard();

    $("#resubmit_modal").on("navigate", (e, navDir, stepNumber) => {
        if($("#resubmit_modal").attr("data-current-step") == 3){
            let quick_form = $("form#quickForm").serializeArray();
            console.log(quick_form);
            let related = "";
            let due_date = "";
            let desc = "";
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
                if(!quickFormIsEmpty){
                    if(validator.form()){
                        $(".btn-success").prop('disabled', false);
                    }
                }
            }
            // validator.form();
        }
    });

    var validator = $('#quickForm').validate({
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
    
    $("#quickForm").on("click keypress",() => { 
        if(!quickFormIsEmpty){
            if(validator.form()){$(".btn-success").prop('disabled', false);}
        }
    });

    $.each(DepartmentLists, (key,val) => {
        if($(`.${val.Name}`).length == $(`.${val.Name}:checked`).length){
            $(`#${val.Name}`).prop('checked', true);
            relatedValidate();
        }
        // console.log(DepartmentLists);
        $(`#${val.Name}`).change(function() {
            if (this.checked) {
                $(`.${val.Name}`).each(function() {
                    this.checked=true;
                });
            } else {
                $(`.${val.Name}`).each(function() {
                    this.checked=false;
                });
            }
            relatedValidate();
        });
    
        $(`.${val.Name}`).click(function () {
            if ($(this).is(":checked")) {
                var isAllChecked = 0;
                $(`.${val.Name}`).each(() => { if(!this.checked) isAllChecked = 1; });
                if(isAllChecked == 0) $(`#${val.Name}`).prop("checked", true);     
            }
            else {
                $(`#${val.Name}`).prop("checked", false);
            }
            relatedValidate();
        });
    });

    function relatedValidate(){
        if($('input:checkbox.qForm.checkSingle:checked').length > 0){
            $(".btn-success").prop('disabled', false);
        }else{
            $(".btn-success").prop('disabled', true);
        }
    }

    $('[data-toggle="datepicker"]').datepicker({
        format: 'dd-mm-yyyy'
    });
    
   

    $("form#Review").submit((e) => {
        e.preventDefault();
        $('#loading').removeClass('hidden')
        SerializeReviewForm();

        $.post(InsertReviewPath, () => {
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
                    url: InsertFilePath,
                    data: Data,
                    cache: false,
                    processData: false,
                    contentType: false,
                    success: function () {
                    },
                    error: function() {
                        alert('error handling here');
                    }
                }));
            });

            optimized_arr.forEach(element => {
                promises.push(
                    $.post(InsertReviewItemPath, {
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

    $("form#quickForm").submit((e) => {
        e.preventDefault();
        let quick_form = $("form#quickForm").serialize();
        console.log(quick_form);
        $.post(InsertRelatedPath, quick_form, () =>{
            console.log('Related created');
            $.post(ReqResubmitPath, quick_form, (res) =>{
                console.log('Resubmit created');
                if(res){
                    swal("Success", "Resubmit Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
                }else{
                    swal("Error", "Resubmit Not Success, Please Try Again", "error");
                }
            });
        });
    });

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

            files.forEach(element => {
                var Data = new FormData();
                Data.append("file",element.file);
                Data.append("description",element.description);
                promises.push($.ajax({
                    type: "POST",
                    url: InsertFileResponsePath,
                    data: Data,
                    cache: false,
                    processData: false,
                    contentType: false,
                    success: function () {
                    },
                    error: function() {
                        alert('error handling here');
                    }
                }));
            });

            promises.push(
                $.post(InsertResponsePath,{ desc: form_response[0].value, resubmit_id: response_id },() => {
                    console.log('Inserted item');
                }).fail(() => {
                alert('error handling here');
            }))

            
            Promise.all(promises).then(() => {
                $('#loading').addClass('hidden')
                InsertReviewStatus = false;
                $("#ResponseSubmit").prop("disabled",true)
                swal("Success", "Insert Complete", "success").then(setTimeout(() => { location.reload(); }, 1500));
            })
    });

    function SerializeReviewForm(){
        var datastring = $("form#Review").serializeArray();
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
                        datastring[i+1].splitted_value = datastring[i+1].name.split('-');
                    }

                    if(datastring[i].splitted_value[0] == "rd"){
                        new_item.id = datastring[i].splitted_value[1];
                        new_item.status = datastring[i].value;
                        if(datastring[i+1].splitted_value[0] == "desc" && datastring[i].splitted_value[1] == datastring[i+1].splitted_value[1]){
                            new_item.description = datastring[i+1].value;
                        }

                    }else if(datastring[i].splitted_value[0] == "desc"){
                        new_item.id = datastring[i].splitted_value[1];
                        new_item.description = datastring[i].value;
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

                if(new_item.id == 999 && new_item.status == null && new_item.description == null){
                    console.error("optimized_arr error");                
                }else{
                    this.optimized_arr.push(new_item);
                }
            }
        }

        console.log(optimized_arr);
    }
    $(".zoom-fab#change_status").click(() => {
        var change_status = "Change status from";
        var new_status = 0;
        if(topic_status == "Review"){
            change_status += " Review to Trial";
            new_status = 9;
        }else if(topic_status == "Trial"){
            change_status += " Trial to Closed";
            new_status = 6;
        }
        swal({
            title: "Change Status", 
            text: change_status, 
            icon:"warning",
        }).then((isChanged) => {
            if(isChanged){
                $.post("/detail/UpdateTopicStatus", {topic_id:topic_id,status:new_status}, (data) => {
                    if(data){
                        swal("Success", "Change Status Success", "success").then(location.reload());
                    }else{
                        swal("Error", "User Password Not Correct", "error");
                    }
                },"json");
            }
        });
    });

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
    })


});
