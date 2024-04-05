def runAll()
{
    def configuredBuild = load "ci/jenkins/configuredBuild.groovy";
    configuredBuild.runAll();
}

pipeline
{
    agent { label 'docker' }

    stages { stage('All') { steps { container('docker') { script { runAll(); } } } } }
}
