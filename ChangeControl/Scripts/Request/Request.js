var submit_Related;
var submit_file;
var remove;
var sendmail;
var GetSummernote;
var test;
var isEditMode;
var TopicStatus = false;
var DepartmentLists;
var InsertRequestPath;
var InsertRelatedPath;
var InsertFilePath;
var file_list = [];
$(document).ready(function () {

    isEditMode = (isEditMode == "True" )? true : false;
    var spanSubmit = $('.pTop');

    spanSubmit.on('click', () => {
        $(this).submit();
    });

    $('input[name="changeType"]').change((e) => {
        let val = e.target.value;
        console.log(`changeType val : ${val}`);
        if(val == "Internal"){
            $("#iTopic").removeClass('hidden');
            $("#eTopic").addClass('hidden');
        }else if(val == "External"){
            $("#iTopic").addClass('hidden');
            $("#eTopic").removeClass('hidden');
        }            
        
    })

    $.each(DepartmentLists, (key,val) => {
        if($(`.${val.Name}`).length == $(`.${val.Name}:checked`).length){
            $(`#${val.Name}`).prop('checked', true);
        }
        $(`#${val.Name}`).change(function() {
            if(this.checked) {
                $(`.${val.Name}`).each(function() {
                    this.checked=true;
                });
            }else{
                $(`.${val.Name}`).each(function() {
                    this.checked=false; 
                });
            }
        });
    
        $(`.${val.Name}`).click(function () {
            if($(this).is(":checked")) {
                var isAllChecked = 0;
    
                $(`.${val.Name}`).each(function() {
                    if (!this.checked) isAllChecked = 1;
                });
    
                if(isAllChecked == 0) {
                   $(`#${val.Name}`).prop("checked", true);
                }     
            }else{
                $(`#${val.Name}`).prop("checked", false); 
            }
        });
    });
    

    $("form.Request").submit((e) => {
        e.preventDefault();
        
        let QC1 = $("input#29").prop("checked");
        let QC2 = $("input#30").prop("checked");
        let QC3 = $("input#31").prop("checked");
            
        let PE1_Process = $("input#32").prop("checked");
        let PE2_Process = $("input#33").prop("checked");

        let isInternal = $("#type_internal").is(":checked");
        let isExternal = $("#type_external").is(":checked");


        if(isInternal){
            if(!(PE1_Process || PE2_Process) || !(QC1 || QC2 || QC3)){ //Need to select PE_Process or QC as Auditor at lease one
                swal("Error", "Please select PE_Process and QC at least one", "error");
                return;
            }
        }else if(isExternal){
            if(!(QC1 || QC2 || QC3)){ //Need to select QC as Auditor at lease one
                swal("Error", "Please select QC at least one", "error");
                return;
            }
        }
        

        let form = SerializeReviewForm();

        files = file_list;
        for(var index in files){
            files[index].file = files[index].detail.file;
            delete files[index].detail;
        }
        $("#loading").removeClass('hidden');
        var inserted_id = "ER-0000000";
        $.post(InsertRequestPath, form, (id) => {
                console.log('Topic created');
                inserted_id = id;
                $.post(InsertRelatedPath, form, () => {
                    console.log('Related created');
                    var promises = [];
                    console.log('files: ',files);
                    
                    files.forEach(element => {
                        if(element.id != null){
                            promises.push(
                                $.post(
                                    UpdateFilePath, {'id': element.id, 'description': element.description}
                                ).fail((xhr, status, error) => {
                                    var errorMessage = xhr.status + ': ' + xhr.statusText
                                    alert('Error - ' + errorMessage);
                                })
                            );
                        }else{
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
                                error: function(xhr, status, error) {
                                    var errorMessage = xhr.status + ': ' + xhr.statusText
                                    alert('Error - ' + errorMessage);
                                }
                                }));
                        }
                    });
                    
                    Promise.all(promises).then(() => {
                        $("#loading").addClass('hidden');
                        $("#ReviewSubmit").prop("disabled",true)
                        swal("Success \n NO. " + inserted_id + "", "Complete Transaction", "success").then(setTimeout(() => { RedirectToDetail(inserted_id); }, 1500));;
                    })
                    
                }).fail(() => {
                    $("#loading").addClass('hidden');
                    swal("Error", "Please try again", "error");
                }).catch(e => {
                    console.log(e);
                });


        }).fail(() => {
            alert('error handling here');
        })
    });

    $("form.Edit").submit((e) => {
        e.preventDefault();
            let form = SerializeReviewForm();
            let topic_id = $("form.Edit").attr('id');
            console.log('form: ',form);
            files = file_list;
            for(var index in files){
                files[index].file = files[index].detail.file;
                delete files[index].detail;
            }

            $("#loading").removeClass('hidden');
            var inserted_id = "ER-0000000";
            $.post(UpdateRequestPath,form, (id) => {
                    console.log('Topic created');
                    inserted_id = id;
                    $.post(InsertRelatedPath, form, () =>{
                        console.log('Related created');
                        var promises = [];
                        
                        files.forEach(element => {
                            if(element.id != null){
                                promises.push($.post(UpdateFilePath, {id: element.id, description: element.description}, () => {
                                    }).fail((xhr, status, error) => {
                                        var errorMessage = xhr.status + ': ' + xhr.statusText
                                        alert('Error - ' + errorMessage);
                                    }));
                            }else{
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
                                    error: function(xhr, status, error) {
                                        var errorMessage = xhr.status + ': ' + xhr.statusText
                                        alert('Error - ' + errorMessage);
                                    }
                                    }));
                            }
                        });
                        
                        Promise.all(promises).then(() => {
                            $("#loading").addClass('hidden');
                            $("#ReviewSubmit").prop("disabled",true)
                            swal("Success \n NO. " + inserted_id + "", "Complete Transaction", "success").then(setTimeout(() => { RedirectToDetail(inserted_id); }, 1500));;
                        })
                            
                    }).fail(() => {
                        $("#loading").addClass('hidden');
                        swal("Error", "Please try again", "error");
                    }).catch(e => {
                        console.log(e);
                    });
                }).fail((request, status, error) => {
                    console.error(request.responseText);
                });
    });

    function RedirectToDetail(id) {
        window.location.replace(`/Detail/Index/?id=${id}`);
    }
});

function setSubject(subject){
    $(".note-editable:eq(0)").html(subject)
}

function SerializeReviewForm(){
    $("input").prop("disabled",false)
    let form = $("form").serialize();
    return form;
}

function SerializeArrayReviewForm(){
    let form = $("form").serializeArray();
    return form;
}

