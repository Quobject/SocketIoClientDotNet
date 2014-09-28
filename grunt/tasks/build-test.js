module.exports = function (grunt) {

  grunt.registerTask('buildTest',
      'test modules', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      configuration = grunt.config('msbuild_configuration'),
      tasks = [],
      clean_format = os === 'win' ? '{0} start-process ' +
        '-NoNewWindow ' +
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2} /t:clean  /p:Configuration={3} \' ' :
        '{0} {1} /t:clean /p:Configuration={2}',
      build_format = os === 'win' ? '{0} start-process ' +
        '-NoNewWindow ' +
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2} /p:Configuration={3} \' ' :
        '{0} {1} /p:Configuration={2}';

    function addBuildWithTitle(title) {
      var   
        dir_path = string.format('{0}/../../Src/{1}/', __dirname,title),
        csproj = string.format('{0}{1}.csproj', dir_path, title),
        clean = os === 'win' ? string.format(clean_format, config.win.powershell, config.win.msbuild, csproj, configuration ):
          string.format(clean_format, config.linux.msbuild, csproj, configuration),
        build = os === 'win' ? string.format(build_format, config.win.powershell, config.win.msbuild, csproj, configuration ):
          string.format(build_format, config.linux.msbuild, csproj, configuration),
        template_file_content = fs.readFileSync('./templates/AssemblyInfo.cs');

      template_file_content = S(template_file_content).replaceAll('@VERSION@', config.version).s;
      //grunt.log.writeln('template_file_content = "%s"', template_file_content);
      fs.writeFileSync(string.format('{0}Properties/AssemblyInfo.cs', dir_path), template_file_content);

      tasks.push(clean);
      tasks.push(build);    
    }

    if (os === 'win') {
      addBuildWithTitle('SocketIoClientDotNet.Tests.net45');
    } else {
      addBuildWithTitle('SocketIoClientDotNet.Tests.mono');
    }

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');
  });
};


