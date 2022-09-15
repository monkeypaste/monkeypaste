// these functions wrap window bindings that return data from host

function getAllTemplatesFromDb_get() {
    let all_templates = [];
    if (typeof getAllTemplatesFromDb === 'function') {
        let allTemplatesJsonStr = getAllTemplatesFromDb();
        log('templates from db:');
        log(allTemplatesJsonStr);
        all_templates = JSON.parse(allTemplatesJsonStr);
    }
    return all_templates;
}

function getDragData_get() {

}