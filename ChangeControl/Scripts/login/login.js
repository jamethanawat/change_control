$(document).ready(function () {

    document.getElementById("user").addEventListener("keyup", function(event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            document.getElementById("pass").focus();
        }
    });

    document.getElementById("pass").addEventListener("keyup", function(event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            $("#click").click();
        }
    });

    $("#click").click(function (event) {
        event.preventDefault();

        var Iuser = $('#user').val();
        var Ipass = $('#pass').val();

        // var select = CreateDepartmentOption();
        // select.onchange = function selectChanged(e) {
        //     value = e.target.value
        // }
        

        console.log(Iuser.length);
        console.log(Ipass.length);

        if (Iuser.length == 0 || Ipass.length == 0) {
            swal("Error", "User Password Not Correct", "error");
        }else{
            // if(Iuser == "63014" && (Ipass == "OPERATOR" || Ipass == "STAFF" || Ipass == "MANAGER")){
            //     event.preventDefault();

            //     swal({
            //         title: "Developer Mode", 
            //         text: "Please submit Department",
            //         content: select,
            //         icon:"warning",
            //     }).then(() => {
            //         let typedDepartment = $(".select-custom").children("option:selected").text();
            //         ajaxAdmin(Iuser, Ipass, typedDepartment);
            //     });
            // }else{
            //     ajaxUser(Iuser, Ipass);
            // }
                ajaxUser(Iuser, Ipass);
                 



            function ajaxUser(Iuser, Ipass){
                $.post(CheckUserPath,({ username:Iuser,password:Ipass }) ,(res) => {
                    if(res.status == "success"){
                        var select = CreateDepartmentOption(res.data);
                        select.onchange = function selectChanged(e) { value = e.target.value }
                        swal({
                            title: "Confirm Department", 
                            text: "Please select your Department",
                            content: select,
                            icon:"warning",
                        }).then(() => {
                            let selected_dept = $(".select-custom").children("option:selected").text();
                            $.post(SetDepartmentAltPath,{dept:selected_dept},(success) => {
                                if(success){
                                    window.location.href = NavigateToHome;
                                }
                            });
                        });
                    }else{
                        if(res.status == "wrong_us"){
                            swal("Error", "Username is wrong.", "error");
                        }else if(res.status == "wrong_pwd"){
                            swal("Error", "Password is wrong.", "error");
                        }else if(res.status == "error"){
                            swal("Error", "Something is wrong, Please contact admin. ", "error");
                        }
                    }
                });
                // swal("Error", "Cannot Not Connect Database", "error");
            }
            
            function ajaxAdmin(Iuser, Ipass, Idepartment){
                $.ajax({
                    type: "POST",
                    url: CheckAdminPath,
                    data: JSON.stringify({
                        username: Iuser,
                        password: Ipass,
                        department: Idepartment,
                    }),
                    contentType: 'application/json; charset=utf-8',
                    success: function (response) {           
                        if (response != "Error") {
                            console.log("gotoHome");
                            window.location.href = response.Url;
                        }else{
                            swal("Error", "User Password Not Correct", "error");
                        }
                    },
                    error: function () {
                        swal("Error", "Cannot Not Connect Database", "error");
                    }
                });
            }

               

        }

    });


});
