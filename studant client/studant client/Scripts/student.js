$("#add_file").click(function () {
    window.location = "/Home/AddFile/" + $(this).data("id");
});

$("#update").click(function () {
    let id = $(this).data("id");
    let d = {
        id: id, first_name: $("#first_name").val(), last_name: $("#last_name").val(), email: $("#email").val(),
        number: $("#number").val(), address: $("#address").val(), birthday: $("#birthday").val(), phone: $("#phone").val()
    }
    $.ajax({
        url: "/Home/save",
        type: "POST",
        data: d,
        success: function () { alert("saved"); window.location.replace(id > 0 ? "/Home/Student?student_id=" + id : "/Home/Studants") },
        error: function () { alert("not saved"); }
    });
});

$("#delete_student").click(function () {
    $.ajax({
        url: "/Home/delete_student/" + $(this).data("id"),
        type: "POST",
        success: function () { alert("deleted"); window.location.replace("/Home/Studants") }
    });
});

$(".downlaod").click(function () {
    window.location = "/Home/download/" + $(this).data("id");
});

$(".delete").click(function () {
    $.ajax({
        url: "/Home/delete_file/" + $(this).data("id"),
        type: "POST",
        success: function () { alert("deleted"); window.location.reload() }
    });
});