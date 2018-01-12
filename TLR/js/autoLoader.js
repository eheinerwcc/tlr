jQuery(function($) {
  $('input.budgetPercentEntry').focus(function() {
    $('input.budgetPercentEntry').autoNumeric();
  });

});