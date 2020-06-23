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

        var select = CreateDepartmentOption();
        select.onchange = function selectChanged(e) {
            value = e.target.value
        }
        

        console.log(Iuser.length);
        console.log(Ipass.length);

        if (Iuser.length == 0 || Ipass.length == 0) {
            swal("Error", "User Password Not Correct", "error");
        }else{
            if(Iuser == "63014" && (Ipass == "OPERATOR" || Ipass == "STAFF" || Ipass == "MANAGER")){
                event.preventDefault();

                swal({
                    title: "Developer Mode", 
                    text: "Please submit Department",
                    content: select,
                    icon:"warning",
                }).then(() => {
                    let typedDepartment = $(".select-custom").children("option:selected").text();
                    ajaxAdmin(Iuser, Ipass, typedDepartment);
                });
            }else{
                ajaxUser(Iuser, Ipass);
            }


            function ajaxUser(Iuser, Ipass){
                $.ajax({
                    type: "POST",
                    url: "/login/CheckUser/",
                    data: JSON.stringify({
                        username: Iuser,
                        password: Ipass,
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
            
            function ajaxAdmin(Iuser, Ipass, Idepartment){
                $.ajax({
                    type: "POST",
                    url: "/login/CheckAdmin/",
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
