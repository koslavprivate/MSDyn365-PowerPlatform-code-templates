var ezn = window.ezn || {}
ezn.PAProjectVCDForm = ezn.PAProjectVCDForm || {}

var Xrm = Xrm || window.parent.Xrm

ezn.PAProjectVCDForm.windowsOnload = async function () {
    var paProjectId = ezn.PAProjectVCDForm.GetCurrentPAProjectvcdID();
    var arrayData = await ezn.PAProjectVCDForm.GetAllFilesFromSharePoint(paProjectId);
    var dropdownHtml = ezn.PAProjectVCDForm.GenerateDropdownHTML(arrayData);
    var outputDiv = document.getElementById("idRecord");
    outputDiv.innerHTML = dropdownHtml;
}

ezn.PAProjectVCDForm.windowsOnChange = function () {
    var newUrl = ezn.PAProjectVCDForm.GetUrlValueFromDropdownHTML();
    var entityId = ezn.PAProjectVCDForm.GetCurrentPAProjectvcdID();
    ezn.PAProjectVCDForm.WriteValueFromDropdownToField(entityId, newUrl);
}


ezn.PAProjectVCDForm.GetCurrentPAProjectvcdID = function () {
    var id = Xrm._page._entityReference.id.guid;
    return id;
};

ezn.PAProjectVCDForm.GetUrlValueFromDropdownHTML = function () {
    let dropdown = document.getElementsByName("picUrls");
    return dropdown[0].value;
};

ezn.PAProjectVCDForm.WriteValueFromDropdownToField = function (entityId, radioValuePicUrl) {
    if (radioValuePicUrl == null) {
        return;
    }
    var data =
    {
        ezn_sppictures: radioValuePicUrl
    };

    Xrm.WebApi.updateRecord("ezn_paprojectvcd", entityId, data).then(
        function success(result) {
            console.log("Picture is changed");
        },
        function (error) {
            console.log(error.message);
        });
};

ezn.PAProjectVCDForm.GenerateDropdownHTML = function (arrayData) {
    var dropdown;
    if (arrayData.length == 0) {
        dropdown = 'No picture was uploaded!';
    }
    else {
        dropdown = '<label for="picUrls">Select picture, please: </label>' +
            '<select id="picUrls" name="picUrls">';
        for (var i = 0; i < arrayData.length; i++) {

            dropdown += '<option value="' + arrayData[i].absoluteurl + '">' + arrayData[i].fullname + '</option>';
        }
        dropdown += '</select>';
    }

    return dropdown;
};


ezn.PAProjectVCDForm.GetAllFilesFromSharePoint = async function (paprojectvcdId) {
    var fetchXml = "?fetchXml=<fetch mapping='logical'>" +
        "<entity name='sharepointdocument'>" +
        "<attribute name='documentid' />" +
        "<attribute name='fullname' />" +
        "<attribute name='relativelocation' />" +
        "<attribute name='sharepointcreatedon' />" +
        "<attribute name='filetype' />" +
        "<attribute name='modified' />" +
        "<attribute name='sharepointmodifiedby' />" +
        "<attribute name='title' />" +
        "<attribute name='author' />" +
        "<attribute name='sharepointdocumentid' />" +
        "<attribute name='readurl' />" +
        "<attribute name='editurl' />" +
        "<attribute name='ischeckedout' />" +
        "<attribute name='absoluteurl' />" +
        "<attribute name='locationid' />" +
        "<attribute name='iconclassname' />" +
        "<order attribute='relativelocation' descending='false' />" +
        "<link-entity name='ezn_paprojectvcd' from='ezn_paprojectvcdid' to='regardingobjectid' link-type='inner' alias='aa'>" +
        "<filter type='and'>" +
        "<condition attribute='ezn_paprojectvcdid' operator='eq' uitype='ezn_paprojectvcd' value='" + paprojectvcdId + "' />" +
        "</filter>" +
        "</link-entity>" +
        "<filter type='or'>" +
        "<condition attribute='filetype' operator='eq' value='png' />" + //add necessary file types
        //"<condition attribute='filetype' operator='eq' value='docs' />" +
        "</filter>" +
        "</entity>" +
        "</fetch>";

    var allRelatedDocuments = Xrm.WebApi.retrieveMultipleRecords("sharepointdocument", fetchXml).then(
        function success(result) {
            return result.entities;
        },
        function (error) {
            return null;
        }
    );
    return allRelatedDocuments;
}


