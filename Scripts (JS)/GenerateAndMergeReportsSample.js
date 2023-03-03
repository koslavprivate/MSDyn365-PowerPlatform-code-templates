var sample = window.sample || {}
sample.EntityRibbon = sample.EntityRibbon || {}

sample.EntityRibbon.generateReport = function (formContext, action) {
    Xrm.Utility.showProgressIndicator('Generate report...')
    loadJS()

    let globalContext = Xrm.Utility.getGlobalContext()
    let clientUrl = globalContext.getClientUrl()
    let orgUniqueName = globalContext.getOrgUniqueName()

    let reportName1 = 'Sample Report Part 1'
    let reportGuid1 = '00000000-0000-0000-0000-000000000001'
    let promise1 = runReportToPrint(reportName1, reportGuid1)

    let reportName3 = 'Sample Report Part 3'
    let reportGuid3 = '00000000-0000-0000-0000-000000000002'
    let promise3 = runReportToPrint(reportName3, reportGuid3)

    Promise.all([promise1, promise3]).then(values => {
        getPdfPageCount(values[1]).then((thirdPartPageCount) => {
            let reportName2 = 'Sample Report Part 2'
            let reportGuid2 = '00000000-0000-0000-0000-000000000003'
            let promise2 = runReportToPrint(reportName2, reportGuid2, [{ key: 'ThirdPartPageCount', value: thirdPartPageCount ?? 0 }])
            promise2.then(value => {
                mergePdfs([values[0], value, values[1]]).then((file) => {
                    let filename = `Sample Report.pdf`
                    if (action === 'preview') {
                        openPreview(file, filename)
                        Xrm.Utility.closeProgressIndicator()
                    }
                    else if (action === 'email') {
                        createEmail(formContext, file, filename)
                    }
                    else {
                        saveData(file, filename)
                        Xrm.Utility.closeProgressIndicator()
                    }
                }).catch(function (error) {
                    console.log(error)
                    Xrm.Utility.closeProgressIndicator()
                })
            }).catch(function (error) {
                console.log(error)
                Xrm.Utility.closeProgressIndicator()
            })
        }).catch(function (error) {
            console.log(error)
            Xrm.Utility.closeProgressIndicator()
        })
    }).catch(function (error) {
        console.log(error)
        Xrm.Utility.closeProgressIndicator()
    })

    function loadJS() {
        if (typeof PDFLib !== 'undefined')
            return
        let script1 = document.createElement('script')
        script1.src = 'https://cdn.jsdelivr.net/npm/pdf-lib/dist/pdf-lib.js'
        document.head.appendChild(script1)
        let script2 = document.createElement('script')
        script2.src = 'https://cdn.jsdelivr.net/npm/pdf-lib/dist/pdf-lib.js'
        document.head.appendChild(script2)
    }

    function runReportToPrint(reportName, reportGuid, additionalParams) {
        return getReportingSession(reportName, reportGuid, additionalParams).then(function (params) {
            let newPth = `${clientUrl}/Reserved.ReportViewerWebControl.axd?ReportSession=${params[0]}&Culture=1033&CultureOverrides=True&UICulture=1033&UICultureOverrides=True&ReportStack=1&ControlID=${params[1]}&OpType=Export&FileName=test&ContentDisposition=OnlyHtmlInline&Format=PDF`
            return getRequest(newPth).then(function (e) {
                return encodePdf(params)
                    .then(function (e) {
                        let binary = ''
                        let bytes = new Uint8Array(e.target.response)
                        for (let i = 0; i < bytes.byteLength; i++) {
                            binary += String.fromCharCode(bytes[i])
                        }
                        let bdy = btoa(binary)
                        return bdy
                    }, function (e) { })
            }, function (e) { })
        }, function (e) { })
    }

    function getRequest(url) {
        return new Promise(function (resolve, reject) {
            let xhr = new XMLHttpRequest()
            xhr.open('get', url, true)
            xhr.onload = resolve
            xhr.onerror = reject
            xhr.send()
        })
    }

    function getReportingSession(reportName, reportGuid, additionalParams) {
        return new Promise(function (resolve, reject) {
            let additionalParamsStr = ''
            if (additionalParams) {
                for (let param of additionalParams) {
                    additionalParamsStr += `&p:${param.key}=${param.value}`
                }
            }

            let recordId = formContext.data.entity.getId()
            recordId = recordId.replace('{', '').replace('}', '')

            let strParameterXML = `<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'><entity name='entity'><all-attributes /><filter type='and'><condition attribute='entityid' operator='eq' value='${recordId}' /> </filter></entity></fetch>`
            let pth = clientUrl + '/CRMReports/rsviewer/reportviewer.aspx'
            let xhr = new XMLHttpRequest()

            xhr.open('POST', pth, true)
            xhr.setRequestHeader('Accept', '*/*')
            xhr.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded')
            xhr.onload = resolve
            xhr.onerror = reject
            xhr.send(`id=%7B${reportGuid}%7D&uniquename=${orgUniqueName}&iscustomreport=true&reportnameonsrs=&reportName=${reportName}&isScheduledReport=false&p:CRM_entity=${strParameterXML}${additionalParamsStr}`)
        }).then(function (e) {
            let x = e.target.responseText.lastIndexOf('ReportSession=')
            let y = e.target.responseText.lastIndexOf('ControlID=')

            let ret = new Array()
            ret[0] = e.target.responseText.substr(x + 14, 24)
            ret[1] = e.target.responseText.substr(x + 10, 32)

            return ret
        }, function (e) {
        })
    }

    function encodePdf(responseSession) {
        return new Promise(function (resolve, reject) {
            let xhr = new XMLHttpRequest()
            let pth = `${clientUrl}/Reserved.ReportViewerWebControl.axd?ReportSession=${responseSession[0]}&Culture=1033&CultureOverrides=True&UICulture=1033&UICultureOverrides=True&ReportStack=1&ControlID=${responseSession[1]}&OpType=Export&FileName=Public&ContentDisposition=OnlyHtmlInline&Format=PDF`
            xhr.open('GET', pth, true)
            xhr.setRequestHeader('Accept', '*/*')
            xhr.responseType = 'arraybuffer'
            xhr.onload = resolve
            xhr.onerror = reject
            xhr.send()
        })
    }

    async function mergePdfs(base64Files) {
        if (typeof PDFLib === 'undefined') {
            return new Promise((resolve) => {
                setTimeout(() => {
                    resolve(mergePdfs(base64Files))
                }, 100)
            })
        }
        const mergedPdf = await PDFLib.PDFDocument.create()
        for (const base64File of base64Files) {
            try {
                if (base64File.length < 2000)
                    continue
                const pdf = await PDFLib.PDFDocument.load(base64File)
                const copiedPages = await mergedPdf.copyPages(pdf, pdf.getPageIndices())
                copiedPages.forEach((page) => {
                    mergedPdf.addPage(page)
                })
            }
            catch (ex) {
                console.log(ex)
            }
        }
        const mergedPdfFile = await mergedPdf.save()
        return mergedPdfFile
    }

    async function getPdfPageCount(base64File) {
        if (typeof PDFLib === 'undefined') {
            return new Promise((resolve) => {
                setTimeout(() => {
                    resolve(getPdfPageCount(base64File))
                }, 100)
            })
        }
        const pdf = await PDFLib.PDFDocument.load(base64File)
        const pageCount = await pdf.getPageCount()
        return pageCount
    }

    function blobToBase64(blob) {
        return new Promise((resolve, _) => {
            const reader = new FileReader();
            reader.onloadend = () => resolve(reader.result);
            reader.readAsDataURL(blob)
        })
    }

    function saveData(data, fileName) {
        let a = document.createElement('a')
        document.body.appendChild(a)
        a.style = 'display: none'
        let blob = new Blob([data], { type: 'octet/stream' }),
            url = window.URL.createObjectURL(blob)
        a.href = url
        a.download = fileName
        a.click()
        window.URL.revokeObjectURL(url)
    }

    function openPreview(data) {
        let blob = new Blob([data], { type: 'application/pdf' })
        let reader = new FileReader()
        reader.readAsDataURL(blob)
        reader.onloadend = function () {
            let base64data = reader.result
            let pageInput = {
                pageType: 'custom',
                name: 'sample_page',
                recordId: base64data
            }
            let navigationOptions = {
                target: 2,
                position: 1,
                width: {
                    value: 50,
                    unit: '%'
                },
                title: 'Sample Page Title'
            }
            Xrm.Navigation.navigateTo(pageInput, navigationOptions)
        }
    }

    async function createEmail(formContext, data) {
        let blob = new Blob([data], { type: 'application/pdf' })
        let base64data = (await blobToBase64(blob)).split(',')[1]
        Xrm.Utility.showProgressIndicator('Create E-Mail...')
        let userSettings = Xrm.Utility.getGlobalContext().userSettings
        let userId = userSettings.userId
        customer = formContext.getAttribute('customerid').getValue()
        let customerId = customer ? customer[0].id : ''
        let customerLogicalName = customer ? customer[0].entityType : ''
        let entityId = formContext.data.entity.getId()
        let from = {
            '@odata.type': 'Microsoft.Dynamics.CRM.systemuser',
            'systemuserid': userId
        }
        let to = {
            '@odata.type': `Microsoft.Dynamics.CRM.${customerLogicalName}`,
            [`${customerLogicalName}id`]: customerId
        }
        let entity = {
            '@odata.type': 'Microsoft.Dynamics.CRM.entity',
            'entityid': entityId
        }
        let request = {
            PDFBody: base64data,
            From: from,
            Target: entity
        }
        if (to)
            request.To = to
        request.getMetadata = function () {
            return {
                boundParameter: null,
                parameterTypes: {
                    'PDFBody': {
                        'typeName': 'Edm.String',
                        'structuralProperty': 1
                    },
                    'From': {
                        'typeName': 'mscrm.systemuser',
                        'structuralProperty': 5
                    },
                    'To': {
                        'typeName': 'mscrm.contact',
                        'structuralProperty': 5
                    },
                    'Target': {
                        'typeName': 'mscrm.entity',
                        'structuralProperty': 5
                    }
                },
                operationType: 0,
                operationName: 'sample_SendPDFByEmail'
            }
        }
        return Xrm.WebApi.online.execute(request).then(
            function (response) {
                if (response.ok) {
                    response.json().then((email) => {
                        console.log(email.activityid)
                        let pageInput = {
                            pageType: 'entityrecord',
                            entityName: 'email',
                            entityId: email.activityid
                        }
                        let navigationOptions = {
                            target: 1
                        }
                        Xrm.Navigation.navigateTo(pageInput, navigationOptions).then(() => {
                            Xrm.Utility.closeProgressIndicator()
                        })
                    })
                }
            },
            function (error) {
                console.log('Error occcurred in executing Global Action')
                console.log(JSON.stringify(error))
                Xrm.Utility.closeProgressIndicator()
            })
    }
}