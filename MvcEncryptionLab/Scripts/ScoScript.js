
function promptForKey() {
    BootstrapDialog.show({
        title: 'ACA-IRS File Processer',
        message: 'Please enter key: <input type="text" class="form-control">',
        buttons: [
            {
                label: 'OK',
                action: function (dialogRef) {
                    var value = dialogRef.getModalBody().find('input').val();
                    if ($.trim(value.toLowerCase()) === '') {
                        alert('Please enter a value or press Cancel.');
                        return false;
                    }
                    else if (/\s/.test(value)) {
                        alert('Value cannot contain any white space (spaces, tabs, line returns, etc.)');
                        return false;
                    }

                    $.ajax({
                        url: "/Default/PostEncryptionKey",
                        type: "POST",
                        data: { 'key': value },
                        dataType: "json",
                        success: function (data) {
                            if (data.status == "success") {
                                BootstrapDialog.show({
                                    title: 'ACA-IRS File Processer',
                                    message: 'Is this phrase accurate?: <input type="text" class="form-control" readonly value="'+data.checkPhrase+'">',
                                    buttons: [
                                        {
                                            label: 'Yes',
                                            action: function (dialogRef) {
                                                $.ajax({
                                                    url: "/Default/PostCheckPhraseResponse/",
                                                    type: "POST",
                                                    data: { 'value': true },
                                                    dataType: "json",
                                                    success: function (data) { }
                                                });
                                            }
                                        },
                                        {
                                            label: 'No',
                                            action: function (dialogRef) {
                                                $.ajax({
                                                    url: "/Default/PostCheckPhraseResponse/",
                                                    type: "POST",
                                                    data: { 'value': false },
                                                    dataType: "json",
                                                    success: function (data) { }
                                                });
                                            }
                                        }
                                    ]
                                });
                            } else {
                                alert("Error occured!");
                            }
                        },
                        error: function (xhr, httpStatusMessage, customErrorMessage) {
                            alert("Status = " + xhr.status + ", Message = " + customErrorMessage);
                        }
                    });
                    dialogRef.close();
                }
            },
            {
                label: 'Cancel',
                action: function (dialogRef) {
                    dialogRef.close();
                }
            }
        ]
    });
}