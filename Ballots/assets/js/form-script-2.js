//hides all conditional fields
$("document").ready(function() {
    $("#order-type-hidden").hide();
    $("#gov-bill-options-hidden").hide();
    $("#ngo-bill-options-hidden").hide();
    $("#shipping-options-hidden").hide();
    $("#pickup-options-hidden").hide();
    $("#additional-address-hidden").hide();
    $("#email-proof-options-hidden").hide();
    $("#hard-proof-options-hidden").hide();
    $("#print-file-hidden").hide();
    $("#authority-declaration").hide();
    $("#reprint-files").hide();
    $(".error-msg").hide();
});


$(function() {
// order type - displays additional fields for reprints and hides them for new orders
     $("#reorder").click(function( event ) {
        $("#order-type-hidden").show();
         $("#reorder-tab").addClass("selected");

    });
    
    $("#new").click(function( event ) {
        $("#order-type-hidden").hide();
        $("#reorder-tab").removeClass("selected");
    });

    
//billing - show additional fields and highlight tabs for each type of billing
    $("#gov").click(function( event ) {
        $("#ngo-bill-options-hidden").hide();
        $("#gov-bill-options-hidden").show();
        $("#gov-bill-tab").addClass("selected");
        $("#ngo-bill-tab").removeClass("selected");
    });
    $("#ngo").click(function( event ) {
        $("#gov-bill-options-hidden").hide();
        $("#ngo-bill-options-hidden").show();
        $("#ngo-bill-tab").addClass("selected");
        $("#gov-bill-tab").removeClass("selected");
    });
  
//spending authority
    $("#authority-yes").click(function( event ) {
    $("#authority-declaration").show();
    });
    
    $("#authority-email").click(function( event ) {
    $("#authority-declaration").hide();
    });
    
    $("#authority-fax").click(function( event ) {
    $("#authority-declaration").hide();
    });
        
        
        
//shipping - show details and hide others
      $("#mail").click(function( event ) { 
      $("#pickup-options-hidden").hide();
      $("#shipping-options-hidden").show();
      $("#add-addresses-hidden").show();
      $("#mail-ship-tab").addClass("selected");
      $("#pickup-ship-tab").removeClass("selected");
    });

//phone for pickup - show details and hide others
        $("#pickup").click(function( event ) { 
        $("#shipping-options-hidden").hide();
        $("#pickup-options-hidden").show();
        $("#pickup-ship-tab").addClass("selected");
        $("#mail-ship-tab").removeClass("selected");
    });
    
//toggle additional address options via add additional addresses link
    $("#add-addresses").click(function( event ) {
          event.preventDefault();
        $("#additional-address-hidden").toggle();
        $("#add-addresses-link").toggleClass("expanded-sm");
    });
    
//proof details - show/hide appropriate sections and highlight appropriate tabs
    $("#hard-proof").click(function( event ) { 
        $("#hard-proof-options-hidden").show();
        $("#email-proof-options-hidden").hide();
        $("#hard-proof-tab").addClass("selected");
        $("#email-proof-tab").removeClass("selected");
    });
    
    $("#email-proof").click(function( event ) { 
        $("#hard-proof-options-hidden").hide();
        $("#email-proof-options-hidden").show();
        $("#email-proof-tab").addClass("selected");
        $("#hard-proof-tab").removeClass("selected");
    });
    
    $("#no-proof").click(function( event ) { 
        $("#hard-proof-options-hidden").hide();
        $("#email-proof-options-hidden").hide();
        $("#hard-proof-tab").removeClass("selected");
        $("#email-proof-tab").removeClass("selected");
    });
    
//print file upload - show and hide
    $("#upload").click(function( event ) { 
        $("#print-file-hidden").show();
//        $("#rough-file-hidden").hide();
    });
    
    $("#later").click(function( event ) { 
    $("#print-file-hidden").hide();
//    $("#rough-file-hidden").hide();
    });
   
    $("#design-help").click(function( event ) { 
    $("#print-file-hidden").hide();
//    $("#rough-file-hidden").show();
    });
 
    
//datepicker
    $( '#datepicker' ).datepicker();
    $( '#previous-date' ).datepicker();
 });

