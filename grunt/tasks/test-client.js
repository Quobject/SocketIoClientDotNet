module.exports = function (grunt) {

  grunt.registerTask('testClient',
      'test cs', function () {
    var
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      tasks = [],
      configuration = grunt.config('msbuild_configuration'),
      test_format_str = os === 'win' ?
        '{0}/xunit.console.exe {1} -nunit test.xml -parallel none' :
        'mono {0}/xunit.console.clr4.exe {1}',

      xunit_path = os === 'win' ?
        config.win.xunit_path : config.linux.xunit_path;

    function addTestDllWithTitle(title) {
      var 
        dir_path = string.format('{0}/../../Src/{1}/', __dirname, title),      
        test_dll = string.format('{0}bin/{1}/{2}.dll', dir_path, configuration, title);

      tasks.push(  string.format(test_format_str,xunit_path, test_dll) );      
    }

    if (os === 'win') {
      addTestDllWithTitle('SocketIoClientDotNet.Tests.net45');
    } else {
      addTestDllWithTitle('SocketIoClientDotNet.Tests.mono');      
    }

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


