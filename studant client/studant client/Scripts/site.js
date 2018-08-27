
//$("#upload_file4").click(function () {
//    var formData = new FormData();
//    $.each(files, function(key, value){
//        formData.append(key, value);
//        $.ajax({
//            url: "http://localhost:51550/api/values/add%20file",
//            type: "POST",
//            data: datajson,
//            dataType: "json",
//            contentType: "appliction/json; charset=utf-8",
//            success: function () { alert("added"); }
//        });
//    });

$(download_files).click(function () {
    $.ajax({
        url: 'http://localhost:64952/Home/download?id=1',
        type: 'Get',
        success: function (data) { alert(data); }
    });
})

//var file;
//$("#filee").change(function (e) { file = e.target.files[0]; })
//$('#Upload1').click(function () {
//    $.ajax({
//        url: 'http://localhost:64952/Home/add%20file',
//        type: 'POST',
//        data: file,
//        dataType: "json",
//        contentType: "appliction/octat-binary; charset=utf-8",
//        success: function () { alert("jkhkj"); }
//    });
//})

/*$(document).ready(function () {
    $.ajax({
        type: "GET",
        url: "http://localhost:51550/api/values",
        success: function (data) {
            let total = 0, perPage = 0, index = 0;
            var $table = $('<table/>').addClass('dataTable table table-bordered table-striped');
            var $header = $('<thead/>').html('<tr><th>ID</th><th>First Name</th><th>Last Name</th><th>User Name</th><th>Email</th></tr>');
            $table.append($header);
            $.each(data, function (i, val) {
                var $row = $('<tr/>');
                $row.append($('<td/>').html(i+1));
                $row.append($('<td/>').html(val.first_name));
                $row.append($('<td/>').html(val.last_name));
                $row.append($('<td/>').html(val.username));
                $row.append($('<td/>').html(val.email));
                $table.append($row);
            });
            $('#panal').html($table);
        },
        error: function (error){
            alert(error);
        },
    });
});*/

