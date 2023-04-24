// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

//$(document).ready(function () {
//    $(".collapse").on("shown.bs.collapse", function () {
//        localStorage.setItem("coll_" + this.id, true);
//    });

//    $(".collapse").on("hidden.bs.collapse", function () {
//        localStorage.removeItem("coll_" + this.id);
//    });

//    $(".collapse").each(function () {
//        if (localStorage.getItem("coll_" + this.id) === "true") {
//            $(this).collapse("show");
//        }
//        else {
//            $(this).collapse("hide");
//        }
//    });
//});

// spinner
$(document).ready(function () {
    $('.spinner').css('display', 'none');

    // init select 2
    $('select').select2();

    // AllServices plus, minus, reset buttons 
    $('.btn-plus, .btn-minus').on('click', function (e) {
        const isNegative = $(e.target).closest('.btn-minus').is('.btn-minus');
        const input = $(e.target).closest('.input-group').find('input');
        const displayText = $(e.target).closest('.input-group').children('.display-text').first().val();
        const ulServices = $('#ul-services');
        const row = $(e.target).closest('tr');

        if (input.is('input')) {
            input[0][isNegative ? 'stepDown' : 'stepUp']()

            ulServices.find('li:contains("' + displayText + '")').remove();

            if (input.val() > 0) {
                row.addClass('table-info')
                ulServices.append('<li>' + input.val()+ ' x ' + displayText + '</li>');
            }
            else
                row.removeClass('table-info')
        }
    })
    $('.btn-reset').on('click', function (e) {
        const input = $(e.target).closest('.input-group').find('input');
        const displayText = $(e.target).closest('.input-group').children('.display-text').first().val();
        const ulServices = $('#ul-services');
        const row = $(e.target).closest('tr');
        if (input.is('input')) {
            input.val(0);
            ulServices.find('li:contains("' + displayText + '")').remove();
            row.removeClass('table-info')
        }
    });

    // AllServices enable/disable button based on room selection
    // add title to list
    $('#ChargeToRoom').on('change', function (e) {
        const selectedValue = e.target.value;
        const selectedText = e.target.selectedOptions[0].text;
        $('#charges-for-title').text('Charges for ' + selectedText);
        if (selectedValue > 0)
            $("#btn-save-charges-modal").removeClass('disabled')
        else 
            $("#btn-save-charges-modal").addClass('disabled')
    });

    // AllServices disabled save button if there is no items selected
    $('#confirm-selected-modal').on('show.bs.modal', function (e) {
        if ($('#confirm-selected-modal ul li').length > 0)
            $('#btn-submit').removeClass('disabled');
        else
            $('#btn-submit').addClass('disabled');
    });
});

//$(window).on('load', function () {
//    $('.spinner').css('display', 'none');
//});


// show spinner when there is an ajax event
$(document).ajaxSend(function (event, xhr, options) {

    $('.spinner').css('display', 'block');

}).ajaxComplete(function (event, xhr, options) {

    $('.spinner').css('display', 'none');

}).ajaxError(function (event, jqxhr, settings, exception) {

    $('.spinner').css('display', 'none');

});

// data tables
$(document).ready(function () {
    $('.data-table').DataTable({
        'language': {
            'paginate': {
                'previous': '<<',
                'next': '>>'
            }
        },
        'lengthMenu': [[10, 25, 50, -1], [10, 25, 50, 'All']]
    });
});
