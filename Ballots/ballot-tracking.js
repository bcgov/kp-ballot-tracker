//starts page with all tables hidden except Step 1
$("document").ready(function() {
        $(".table").hide();
        
        if (document.getElementById('userType').value == "DVS") {
          document.cookie = "all-tab";
        }

        var currTab = document.cookie;

        if (currTab == "qp-tab"){
            $(".view-qp").show();
            $("#qp-tab, #layout-stat").addClass("current");
        }
        else if (currTab == "ebc-tab"){
            $(".view-ebc").show();
            $("#ebc-tab, #proofing-stat").addClass("current");
        }
        else if (currTab == "printer-tab") {
            $(".view-printer").show();
            $("#printer-tab, #printing-stat").addClass("current");
        }
        else if (currTab == "all-tab") {
            $(".view-all").show();
            $("#all-tab").addClass("current");
        }
        else {
            $(".view-upload").show();
            $("#upload-tab, #upload-stat").addClass("current");
        }

    // Function : .upload_queue class for ifive uploads
    // Purpose  : each instance of the upload field requires a queue 
        jQuery(".upload_queue").each(function () {
            var id = $(this).attr('class').replace('upload_queue ', "");
            var uploadType = $(this).attr('name').replace('upload_queue_', "");
            uploadType = uploadType.replace("_" + id, "");
            var region = $(this).attr('id').replace($(this).attr('name') + '_', "");

            //alert("I am an alert box! On Upload");
            jQuery(this).uploadifive({
                'auto': false,
                'fileSizeLimit': '100MB',
                'uploadLimit': 1,
                'queueSizeLimit': 1,
                'formData': { 'ed': id, 'uploadType': uploadType, 'region': region },
                'uploadScript': 'FileHandler.ashx',
                'onUpload': function () {
                    //alert('Upload starting with id:' + id);
                    console.log("Upload starting with id:" + id);
                },
                'onUploadComplete': function () {
                    var firstfile = file.name;
                    //DG .. set this to the username field id
                    var user = document.getElementById("userid").value;
                    //var user = 3;

                    //alert('Upload Succeeded with id:' + id + ' user ' + user + ' and files : ' + firstfile);
                    var result = setEBCUpload(id, user, firstfile, uploadType);
                }

            });
        });


        // Function : #tblJobFileList button.upload_button click
        // Purpose  : Starts ifiveupload
        $("#tracking-table").on("click", "button.upload_button", function () {
            var id = $(this).attr('id').replace('upload_button_', "");
            var jobID = $(this).attr('jobID');

            console.log("Are we here");
            console.log("ID: " + id);

            $('#upload_queue_' + id).uploadifive('upload');
        });

        console.log("Page Loaded");
});

//if any tab is clicked, hides all tables and removes current class from all tabs and adds current class to that tab
$(function() {
     $("#stepTabs li, #progressStats li, #all-ballots").click(function( event ) {
       $("#stepTabs li, #progressStats li").removeClass("current");
       $(".table").hide();
    });
});

$(function() {
// show upload table view
     $("#upload-tab, #upload-stat").click(function( event ) {
         $(".view-upload").show();
         $("#upload-tab, #upload-stat").addClass("current");
         document.cookie = "upload-tab";
    });
});


$(function() {
// show QP table view
     $("#qp-tab, #layout-stat").click(function( event ) {
         $(".view-qp").show();
         $("#qp-tab, #layout-stat").addClass("current");
         document.cookie = "qp-tab";
    });
});

$(function() {
// show EBC table view
     $("#ebc-tab, #proofing-stat").click(function( event ) {
       $(".view-ebc").show();
       $("#ebc-tab, #proofing-stat").addClass("current");
       document.cookie = "ebc-tab";
    });
});
    
$(function() {
// show printer table view
     $("#printer-tab, #printing-stat").click(function( event ) {
        $(".view-printer").show();
        $("#printer-tab, #printing-stat").addClass("current");
        document.cookie = "printer-tab";
    });
});

$(function() {
// shows complete table view
     $("#all-tab").click(function( event ) {
            $(".view-all").show();
            $("#all-tab").addClass("current");
            document.cookie = "all-tab";
    });
});

$(function() {
// open dialog when View Proof button is clicked
$( "#proof-dialog" ).dialog({
    autoOpen: false,
    modal: true,
    height: 800,
    width: 1400,
    scrollTop: 0,
    buttons: {
        "Approve proof": function() {
            $(this).dialog("close");
            currED = document.getElementById('currED').value
            console.log("ED: " + currED)
            var userid = document.getElementById("userid").value;
            approveProof(currED, userid);
        },
        "Proof not approved": function() {
            $(this).dialog("close");
            currED = document.getElementById('currED').value
            console.log("ED: " + currED)
            var userid = document.getElementById("userid").value;
            rejectProof(currED, userid);
        }
    },
    close: function (event, ui) {
        //clear the source so the previously viewed ballot doesn't appear initially when viewing a ballot proof after closing the previous ballot proof with the X close button
        //if file is copied locally to be viewed then need to delete it when the window is closed
        document.getElementById('proof-iframe').src = "FileHandler.ashx?fileAction=delete&userid=" + document.getElementById("userid").value;
    }
});

$(".btn-proof").click(function (value) {
    event.preventDefault();

    document.getElementById('currED').value = value.target.name.substring(0, 3);

    document.getElementById('proof-dialog').scrollTop = 0;

    document.getElementById('proof-iframe').src = "FileHandler.ashx?uploadType=view&ed=" + value.target.name.substring(0, 3) + "&region=" + value.target.id.replace(value.target.name.substring(0, 3) + '-ballot-proof-', "") + "&userid=" + document.getElementById("userid").value;

    $("#proof-dialog").dialog("open");
    $("#proof-dialog").dialog("option", "width", 1280);
    $("#proof-dialog").dialog("option", "height", 700);
    $("#proof-dialog").dialog("option", "title", "Ballot Proof");

  });
});

$(function () {
  // open dialog when View Proof button is clicked
  $("#mail-ballot-dialog").dialog({
    autoOpen: false,
    modal: true,
    height: 800,
    width: 1400,
    scrollTop: 0,
    buttons: {
      "Approve mail-in ballot": function () {
        $(this).dialog("close");
        currED = document.getElementById('mailBallotED').value
        console.log("ED: " + currED)
        var userid = document.getElementById("userid").value;
        approveMailBallot(currED, userid);
      },
      "Mail-in ballot not approved": function () {
        $(this).dialog("close");
        currED = document.getElementById('mailBallotED').value
        console.log("ED: " + currED)
        var userid = document.getElementById("userid").value;
        rejectMailBallot(currED, userid);
      }
    },
      close: function (event, ui) {
        //clear the source so the previously viewed ballot doesn't appear initially when viewing a ballot proof after closing the previous ballot proof with the X close button
        //if file is copied locally to be viewed then need to delete it when the window is closed
        document.getElementById('mail-ballot-iframe').src = "FileHandler.ashx?fileAction=delete&userid=" + document.getElementById("userid").value;
    }
  });

  $(".btn-mail-ballot").click(function (value) {
    event.preventDefault();
 
    document.getElementById('mailBallotED').value = value.target.name.substring(0, 3);

    document.getElementById('mail-ballot-dialog').scrollTop = 0;

    document.getElementById('mail-ballot-iframe').src = "FileHandler.ashx?uploadType=view_mail&ed=" + value.target.name.substring(0, 3) + "&userid=" + document.getElementById("userid").value;

    $("#mail-ballot-dialog").dialog("open");
    $("#mail-ballot-dialog").dialog("option", "width", 1280);
    $("#mail-ballot-dialog").dialog("option", "height", 700);
    $("#mail-ballot-dialog").dialog("option", "title", "Mail-In Ballot");

  });
});

$(function () {
  // open dialog when View Ballot button is clicked
  $("#ballot-dialog").dialog({
    autoOpen: false,
    modal: true,
    height: 800,
    width: 1400,
    scrollTop: 0,
    buttons: {
      "Close": function () {
        $(this).dialog("close");
      }
    }
  });

  $(".btn-ballot").click(function (value) {
    event.preventDefault();

    document.getElementById('viewBallotED').value = value.target.name.substring(0, 3);

    document.getElementById('ballot-dialog').scrollTop = 0;

    document.getElementById('ballot-iframe').src = "FileHandler.ashx?uploadType=view&ed=" + value.target.name.substring(0, 3) + "&region=" + value.target.id.replace(value.target.name.substring(0, 3) + '-ballot-approved-', "") + "&userid=" + document.getElementById("userid").value;

    $("#ballot-dialog").dialog("open");

    $("#ballot-dialog").dialog("option", "width", 1280);
    $("#ballot-dialog").dialog("option", "height", 700);
    $("#ballot-dialog").dialog("option", "title", "Approved Ballot");

  });
});

$(function() {
    $("#tracking-table").tooltip({
        position: { my: "left top", at: "left+5 bottom" }
    });
});

function OnSuccess(response) {
    alert('Success: ' + response.d);
}

function OnError(response) {
    alert('Error: ' + response.d);
}

/*
         Function : setEBCUpload (id, user, path)
         Purpose  : Updates the upload fields in the database via ajax setEBCUpload
         Notes    : 
          
         Returns  : 
         */
function setEBCUpload(id, user, path, uploadType) {
    $.ajax({
        type: "POST",
        url: "ballot-tracker.aspx/setEBCUpload",
      data: "{id :'" + id + "' , user:'" + user + "'" + " , path:'" + path + "', uploadType:'" + uploadType + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {
          refreshPage();
        },
        failure: function (response) {
            // alert(response.d);
        }
    });
};

function approveProof(ed, userid) {
  $.ajax({
    type: "POST",
    url: "ballot-tracker.aspx/ApproveProof",
    data: "{ed :'" + ed + "' , userid:'" + userid + "'}",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (data) {
      refreshPage();
    },
    failure: function (response) {
      // alert(response.d);
    }
  });
};

function rejectProof(ed, userid) {
  $.ajax({
    type: "POST",
    url: "ballot-tracker.aspx/RejectProof",
    data: "{ed :'" + ed + "' , userid:'" + userid + "'}",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (data) {
      refreshPage();
    },
    failure: function (response) {
      // alert(response.d);
    }
  });
};

function approveMailBallot(ed, userid) {
  $.ajax({
    type: "POST",
    url: "ballot-tracker.aspx/ApproveMailBallot",
    data: "{ed :'" + ed + "' , userid:'" + userid + "'}",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (data) {
      refreshPage();
    },
    failure: function (response) {
      // alert(response.d);
    }
  });
};

function rejectMailBallot(ed, userid) {
  $.ajax({
    type: "POST",
    url: "ballot-tracker.aspx/RejectMailBallot",
    data: "{ed :'" + ed + "' , userid:'" + userid + "'}",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (data) {
      refreshPage();
    },
    failure: function (response) {
      // alert(response.d);
    }
  });
};

/*
 Function : approveHardCopy(ed)
 Purpose  : Updates the hardCopyApprove fields in the database via ajax ApproveHardCopy
 Notes    :
 Returns  :
 */
function approveHardCopy(ed) {
  var userid = document.getElementById("userid").value;
  $.ajax({
    type: "POST",
    url: "ballot-tracker.aspx/ApproveHardCopy",
    data: "{ed :'" + ed + "' , userid:'" + userid + "'}",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (data) {
      refreshPage();
    },
    failure: function (response) {
      // alert(response.d);
    }
  });
};



function refreshPage() {
  document.getElementById("ballotsForm").submit();
}

function reUploadExtract(ed) {
  if (confirm("Are you sure?\nClick OK to re-upload the extract on the Waiting for Extract Upload tab.")) {
    $.ajax({
      type: "POST",
      url: "ballot-tracker.aspx/ReUploadExtract",
      data: "{ed :'" + ed + "'}",
      contentType: "application/json; charset=utf-8",
      dataType: "json",
      success: function (data) {
        refreshPage();
      },
      failure: function (response) {
        // alert(response.d);
      }
    });
  }
}


