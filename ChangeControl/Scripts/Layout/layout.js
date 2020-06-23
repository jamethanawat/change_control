var Search;
var GetLine;
$(document).ready(function () {
    // Search all columns
    console.log("layout");

    window.onbeforeunload = function() {
        // $('#loading').show('slow');
        $('#loading').removeClass('hidden');
    };

    $(".nav-item .animsition-link").click(() => {
        $('#loading').removeClass('hidden');
    })
});


    //$(document).ready(function () {
    //    //console.log("12222");
    //    // $("#test").addClass('active');
    //    $("#menu1").on("click", function () {
    //        console.log("1");
    //        $("#menu1-1").addClass('active');
    //        //$("#menu2-2").removeClass('active');
    //    });

    //    $("#menu2").on("click", function () {
    //        try {
    //            console.log("2");
    //            $("#test").addClass('active');
    //            $("#menu2-1").addClass('nav-link active');
    //        //$("#menu1-1").removeClass('active');
    //        }
    //        catch (err) {
    //           consoloe.log("ERR-",err.message);
    //        }

          
    //    });
    //});
