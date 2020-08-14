(function(speak) {
    var parentApp = window.parent.Sitecore.Speak.app.findApplication('EditActionSubAppRenderer');
    var designBoardApp = window.parent.Sitecore.Speak.app.findComponent('FormDesignBoard');

    var getFields = function() {
        const fields = designBoardApp.getFieldsData();
        return _.reduce(fields,
            function(memo, item) {
                if (item && item.model && item.model.hasOwnProperty('value')) {
                    memo.push({
                        itemId: item.itemId,
                        name: item.model.name
                    });
                }
                return memo;
            },
            [
                {
                    itemId: '',
                    name: ''
                }
            ],
            this);
    };

    speak.pageCode(['underscore'],
        function(_) {
            return {
                initialized: function() {
                    this.on({ "loaded": this.loadDone }, this);
                    this.Fields = getFields();
                    this.SettingsForm.FieldFirstNameId.on('change:SelectedItem', this.validate, this);
                    this.SettingsForm.FieldLastNameId.on('change:SelectedItem', this.validate, this);
                    this.SettingsForm.FieldEmailAddressId.on('change:SelectedItem', this.validate, this);

                    if (parentApp) {
                        parentApp.loadDone(this, this.HeaderTitle.Text, this.HeaderSubtitle.Text);
                    }
                },
                validate: function() {
                    parentApp.setSelectability(this, false);
                    if (this.SettingsForm.FieldEmailAddressId.SelectedValue !== '') {
                        parentApp.setSelectability(this, true);
                    }
                },
                setFirstNameFieldData: function(listComponent, data, currentValue) {
                    const items = data.slice(0);
                    if (currentValue && !_.findWhere(items, { itemId: currentValue })) {
                        const currentField = {
                            itemId: currentValue,
                            name: currentValue + ' - value not in the selection list'
                        };
                        items.splice(1, 0, currentField);
                        listComponent.DynamicData = items;
                        $(listComponent.el).find('option').eq(1).css('font-style', 'italic');
                    } else {
                        listComponent.DynamicData = items;
                        listComponent.SelectedValue = currentValue;
                    }
                },
                setLastNameFieldData: function(listComponent, data, currentValue) {
                    const items = data.slice(0);
                    if (currentValue && !_.findWhere(items, { itemId: currentValue })) {
                        const currentField = {
                            itemId: currentValue,
                            name: currentValue + ' - value not in the selection list'
                        };
                        items.splice(1, 0, currentField);
                        listComponent.DynamicData = items;
                        $(listComponent.el).find('option').eq(1).css('font-style', 'italic');
                    } else {
                        listComponent.DynamicData = items;
                        listComponent.SelectedValue = currentValue;
                    }
                },
                setEmailFieldData: function(listComponent, data, currentValue) {
                    const items = data.slice(0);
                    if (currentValue && !_.findWhere(items, { itemId: currentValue })) {
                        const currentField = {
                            itemId: currentValue,
                            name: currentValue + ' - value not in the selection list'
                        };
                        items.splice(1, 0, currentField);
                        listComponent.DynamicData = items;
                        $(listComponent.el).find('option').eq(1).css('font-style', 'italic');
                    } else {
                        listComponent.DynamicData = items;
                        listComponent.SelectedValue = currentValue;
                    }
                },
                loadDone: function(parameters) {
                    this.Parameters = parameters || {};
                    this.SettingsForm.setFormData(this.Parameters);
                    this.setFirstNameFieldData(this.SettingsForm.FieldFirstNameId, getFields(), this.Parameters['fieldFirstNameId']);
                    this.setLastNameFieldData(this.SettingsForm.FieldLastNameId, getFields(), this.Parameters['fieldLastNameId']);
                    this.setEmailFieldData(this.SettingsForm.FieldEmailAddressId, getFields(), this.Parameters['fieldEmailAddressId']);
                    this.SettingsForm.FromFieldSection.IsVisible = true;
                    this.validate();
                },
                getData: function() {
                    this.Parameters['fieldFirstNameId'] = this.SettingsForm.FieldFirstNameId.SelectedValue;
                    this.Parameters['fieldLastNameId'] = this.SettingsForm.FieldLastNameId.SelectedValue;
                    this.Parameters['fieldEmailAddressId'] = this.SettingsForm.FieldEmailAddressId.SelectedValue;
                    return this.Parameters;
                }
            };
        });
})(Sitecore.Speak);
