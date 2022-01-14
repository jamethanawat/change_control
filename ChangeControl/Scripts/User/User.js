var dataTable;
var se_dept = '';
$(document).ready(function () {
    
    dataTable = $('#user_tb').DataTable({
        // "paging": true,
        // "lengthChange": true,
         //"searching": true,
        // ordering: false,
        // "info": true,
        // "autoWidth": true,
        // "pageLength": 10,        
        ajax: {
            url: GetUserWithPermissionByDeptPath,
            type: "POST",
            //data: { dept: us_dept, searchdept: se_dept}
            data: function (d) {
                d.dept = us_dept,
                d.searchdept = se_dept;
            }
        },
        treeGrid: {
            left: 10,
            expandIcon: '<span><i class="fas fa-angle-right text-primary"></i></span>',
            collapseIcon: '<span><i class="fas fa-angle-down text-primary"></i></span>'
        },
        order: [[ 1, "asc" ]],
        columns: 
        [
            {
                target: 0,
                className: 'treegrid-control',
                orderable: false,
                data: function (item) {
                    if(item.children != null && item.children.length > 0) return '<span><i class="fas fa-angle-right text-primary"></i></span>';
                    return '';
                }
            },{
                target: 1,
                data: function (item) {
                    if(item.Name == "" || item.Name == null) return '';
                    return item.User;
                }
            },{
                target: 2,
                orderable: false,
                data: function (item) {
                    return (item.Name != null) ? `${item.Name}`+(item.Email ? `<br>${item.Email}` : "") : null ;
                }
            },{ 
                target: 3,
                className: 'center', 
                data: function (item){
                    if(item.Name == "" || item.Name == null) return "";

                    let select_pos = `<select class="form-control select2" name="position" style="width: 100%;" onchange="changePosition(this)" `+ (item.Position == "Admin" && us_pos != "Admin" ? `disabled` : null)  +` >`;
                    item.Position = (item.Position == "Special") ? "Approver" : (item.Position == "S_Issue") ? "Issue" : item.Position;
                    Positions.forEach(pos => {
                        select_pos += `<option user="${item.User}" value="${pos}"`+(pos == item.Position ? "selected" : "")+`>${pos}</option>`;
                    });
                    
                    if(!Positions.includes("Admin") && item.Position == "Admin") select_pos += `<option value="2" selected disabled>Admin</option>`;
                    
                    select_pos += `</select>`;
                    return select_pos;
                } },
            { target: 4,
                className: 'center', 
                data: function (item){
                    let select_dept = `<select class="form-control select2" name="department" style="width: 100%;" onchange="` + (item.Name == "" || item.Name == null ? 'changeDepartment(this)' : 'changeDepartmentAndUser(this)' ) + `" >`;
                    Departments.forEach(dept => {
                        select_dept += `<option user="${item.User}" prev-dept="${item.Dept}" value="${dept}"` + (dept == item.Dept ? "selected" : "") + `>${dept}</option>`;
                    });
                    select_dept += `</select>`;
                    console.log(item.Dept);
                    return select_dept;
                } 
            }
            ,{ 
                target: 5,
                className: 'center', 
                data: function (item){
                    let checked = (item.Subscribe == 1) ? "checked" : "";
                    return `<div class="icheck-primary d-inline">
                                <input type="checkbox" id="sc-${item.User}-${item.Dept}" onchange="changeSubscribe('${item.User}','${item.Dept}',this.checked)" ${checked}>
                                <label for="sc-${item.User}-${item.Dept}">
                                </label>
                            </div>`;
                } 
            },{
                target: 6,
                orderable: false,
                data: function (item) {
                    if(item.Name != null) return `<input type="submit" class="btn btn-block btn-sm btn-danger" user="${item.User}" value="Delete user" onclick="deleteUser(this)">`;
                    return `<input type="submit" class="btn btn-block btn-sm btn-warning" user="${item.User}" department="${item.Dept}" value="Delete permission" onclick="deletePermission(this)">`;
                }
            },
        ],
    });

    $("input[name='user']").mask('00000');  

/* ---------------------------------------------------------------------------------------------- */
/*                                            Add user                                            */
/* ---------------------------------------------------------------------------------------------- */

    $("form#User").submit(function (e) {
        let user_form = $(this).serialize()
        console.log(user_form);
        if(!(user_form.indexOf('=&') > -1)){ //Check empty field
            $.post(AddUserPath , $(this).serialize(), (res) => {
                if(res.status == "success"){
                    $(this).trigger('reset');
                    $.post(GetUserCodeByDeptPath, {dept : us_dept}, (res) => {
                        let pm_select = $("#Permission [name='user']");
                        pm_select.empty();
                        pm_select.append('<option disabled="disabled">Select Position</option>');
                        res.data.forEach((user) => {
                            pm_select.append(`<option value="${user}">${user}</option>`);
                        });
                    }).done(() => {
                        se_dept = '';
                        dataTable.ajax.reload();
                        swal("Success", "Add user success.", "success");
                    });
                }else if(res.status == "duplicated"){
                    swal("Warning", "This user already exists.", "warning");
                }
            }).fail((error) => {
                swal("Error", "Cannot add user, Please try again or contact admin.", "error");
            });
        }else{
            swal("Warning", "Please fill all field.", "warning");
        }
    });

/* ---------------------------------------------------------------------------------------------- */
/*                                         Add permission                                         */
/* ---------------------------------------------------------------------------------------------- */

    $("form#Permission").submit(function (e) {
        let pms_form = $(this).serialize()
        console.log(pms_form);
        if(!(pms_form.indexOf('=&') > -1)){ //Check empty field
            $.post(AddPermissionPath , $(this).serialize(), (res) => {
                if(res.status == "success"){
                    $(this).trigger('reset');
                    se_dept = '';
                        dataTable.ajax.reload();
                        swal("Success", "Add permission success.", "success");
                }else if(res.status == "duplicated"){
                    swal("Warning", "This user already exists.", "warning");
                }
            }).fail((error) => {
                swal("Error", "Cannot add permission, Please try again or contact admin.", "error");
            });
        }else{
            swal("Warning", "Please select all field.", "warning");
        }
    });

/* ---------------------------------------------------------------------------------------------- */
/*                                          On clear user                                         */
/* ---------------------------------------------------------------------------------------------- */

    $("#clear_btn").click(function(e){
        e.preventDefault();
        if(e.which != 13) $(this.form).trigger('reset');
    })

    /* ---------------------------------------------------------------------------------------------- */
    /*                                          On Search Department                                  */
    /* ---------------------------------------------------------------------------------------------- */

    $("#search_department").click(function (e) {

        var select;
        select = CreateDepartmentOption2();
        swal({
            title: "Search By Department",
            text: "Please select Department",
            closeOnClickOutside: true,
            // buttons : [true,true],
            content: select,
            icon: "warning",
        }).then(() => {
            se_dept = $(".select-custom").children("option:selected").text();
            //console.log(se_dept, "select");
            dataTable.ajax.reload();

        });
    })


});


function CreateDepartmentOption2() {
    const select = document.createElement('select');
    select.className = 'select-custom'
    let i = 1;
    $('[name="dept"] option').each(function () {
        let option = document.createElement('option');
        option.innerHTML = $(this).val();
        option.value = i;
        select.appendChild(option);
        i++;
    });
    return select;
}

function changePosition(e){
    console.log(e);
    let opt_us = e.selectedOptions[0].getAttribute("user")
    if((e.value != "" && e.value != null) && (opt_us != "" && opt_us != null)){
        $.post(UpdatePositionPath , {user:opt_us, pos:e.value}, (res) => {
            (res.status == "success") ? notyf.success('Update position complete') : notyf.error('Update position not complete');
        });
    }
}

function changeDepartment(e){
    console.log(e);
    let opt_us = e.selectedOptions[0].getAttribute("user")
    let prev_dept = e.selectedOptions[0].getAttribute("prev-dept")
    if((e.value != "" && e.value != null) && (opt_us != "" && opt_us != null)){
        $.post(UpdateDepartmentPath , {user:opt_us, prev_dept:prev_dept, dept:e.value}, (res) => {
            switch(res.status){
                case "success" : 
                    notyf.success('Update department complete');
                    break;
                case "duplicated" : 
                    notyf.error('Duplicated department')
                    $(e).children(`[value='${prev_dept}']`).prop('selected',true)
                    break;
                case "error" : 
                    notyf.error('Update department not complete')
                    $(e).children(`[value='${prev_dept}']`).prop('selected',true)
                    break;
            }
        });
    }
}

function changeDepartmentAndUser(e){
    console.log(e);
    let opt_us = e.selectedOptions[0].getAttribute("user")
    let prev_dept = e.selectedOptions[0].getAttribute("prev-dept")
    if((e.value != "" && e.value != null) && (opt_us != "" && opt_us != null)){
        $.post(UpdateDepartmentAndUserPath , {user:opt_us, prev_dept:prev_dept, dept:e.value}, (res) => {
            switch(res.status){
                case "success" : 
                    notyf.success('Update department complete');
                    se_dept = '';
                    dataTable.ajax.reload();
                    break;
                case "duplicated" : 
                    notyf.error('Duplicated department')
                    $(e).children(`[value='${prev_dept}']`).prop('selected',true)
                    break;
                case "error" : 
                    notyf.error('Update department not complete')
                    $(e).children(`[value='${prev_dept}']`).prop('selected',true)
                    break;
            }
        });
    }
}

function changeSubscribe(user,dept,val){
    if(user != null, dept != null, val != null){
        $.post(UpdateSubscribePath , {user:user, dept:dept, status: val | 0}, (res) => {
            (res.status == "success") ? notyf.success('Update subscribe complete') : notyf.error('Update subscribe not complete');
        });
    }
}

function deletePermission(e){
    swal({
        title: "Delete permission", 
        text: "Do you want to delete this permission?", 
        closeOnClickOutside: false,
        buttons : [true,true],
        icon:"warning",
    }).then((res) => {
        if(res){
            console.log(e);
            $.post(DeletePermissionPath, {dept:e.getAttribute("department"), user:e.getAttribute("user")}, (res) => {
                if(res.status == "success"){
                    notyf.success('Delete permission success');
                    se_dept = '';
                    dataTable.ajax.reload();
                }else{
                    notyf.error('Delete permission not success');
                }
            });
        }
    });
}

function deleteUser(e){
    swal({
        title: "Delete User", 
        text: "Do you want to delete this User?", 
        closeOnClickOutside: false,
        buttons : [true,true],
        icon:"warning",
    }).then((res) => {
        if(res){
            $.post(DeleteUserPath, {user:e.getAttribute("user")}, (res) => {
                if(res.status == "success"){
                    notyf.success('Delete user success');
                    se_dept = '';
                    dataTable.ajax.reload();
                }else{
                    notyf.error('Delete user not success');
                }
            });
        }
    });
}




