﻿var count = 0;

function show_div(s) {
    console.log(count);
    if (s == "Recipient") {
        if (count == 0)
            $("#append").css("display", "block");
        count++;
    }
    if (s == "Sender" && count > 0) {
        count--;
        if (count == 0)
            $("#append").css("display", "none");
    }
}

$(".btn_click").click(function () {
    show_div($(this).val());
    console.log(count);
});


$(".btn_sa").click(function () {
    console.log("enter");
    if ($(this).val() == "recipient") {
        $("#recipient").css("display", "block");
        $("#pickup").css("display", "none");
    } else if ($(this).val() == "pickup") {
        $("#recipient").css("display", "none");
        $("#pickup").css("display", "block");
    } else if ($(this).val() == "saved_address") {
        $("#new_address").css("display", "none");
        $("#saved_address").css("display", "block");
    } else if ($(this).val() == "new_address") {
        $("#saved_address").css("display", "none");
        $("#new_address").css("display", "block");
    }
});