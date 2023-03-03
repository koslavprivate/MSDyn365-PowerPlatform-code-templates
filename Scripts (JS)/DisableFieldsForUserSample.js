var sample = window.sample || {}
sample.SampleEntityForm = sample.SampleEntityForm || {}

sample.SampleEntityForm.onLoad = function (executionContext) {
    let formContext = executionContext.getFormContext()

    sample.SampleEntityForm.sampleUserRestrictions(formContext)
}

sample.SampleEntityForm.onSave = function (executionContext) {
    let formContext = executionContext.getFormContext()

    sample.SampleEntityForm.sampleUserRestrictions(formContext)
}

sample.SampleEntityForm.sampleUserRestrictions = function (formContext) {
    let isBaseUserCustomized = sample.SampleEntityForm.hasCurrentUserRole('Sample User')
    let isAdmin = sample.SampleEntityForm.hasCurrentUserRole('System Administrator')
    if (!isAdmin && isBaseUserCustomized) {
        sample.SampleEntityForm.disableForm(formContext)
    }
    if (!isAdmin && !isBaseUserCustomized) {
        formContext.getControl("sample_field1").setDisabled(false)
        formContext.getControl("sample_field2").setDisabled(false)
    }
}

sample.SampleEntityForm.hasCurrentUserRole = function (roleName) {
    let hasRole = false
    let roles = Xrm.Utility.getGlobalContext().userSettings.roles
    roles.forEach(x => {
        if (x.name === roleName) {
            hasRole = true
            return
        }
    })
    return hasRole
}

sample.SampleEntityForm.disableForm = function (formContext) {
    let formControls = formContext.ui.controls
    formControls.forEach(control => {
        if (control.getName() !== '' && control.getName() !== null) {
            control.setDisabled(true)
        }
    })
}