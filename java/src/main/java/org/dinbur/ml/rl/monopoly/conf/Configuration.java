package org.dinbur.ml.rl.monopoly.conf;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.InputStream;
import java.util.Properties;


public final class Configuration {

    private static final Logger logger = LoggerFactory.getLogger(Configuration.class);

    private static final Configuration INSTANCE = new Configuration();

    private final Properties properties;

    private Configuration() {
        this.properties = new Properties();
        load();
    }

    private void load() {
        ClassLoader loader = getClass().getClassLoader();
        try (InputStream stream = loader.getResourceAsStream("rl-monopoly.properties")) {
            properties.load(stream);
        } catch (Exception ex) {
            logger.warn("Failed to read configuration for AssignmentStore. Will proceed with the default values", ex);
        }
    }

    /**
     * Returns the singleton  instance for the ConfigurationLoader
     *
     * @return ConfigurationLoader instance
     */
    public static Configuration getInstance() {
        return INSTANCE;
    }

    /**
     * Retrieves a <b>string</b> value from the configuration file.
     * If the provided key is not present, the default value provided will be used.
     *
     * @param propertyName configuration key
     * @return the configuration value found for this key, default value esle
     */
    public String getStringValue(final String propertyName) {
        return properties.getProperty(propertyName);
    }

    /**
     * Retrieves an <b>int</b> value from the configuration file.
     * If the provided key is not present, the default value provided will be used.
     *
     * @param propertyName configuration key
     * @return the configuration value found for this key, default value esle
     */
    public Integer getIntValue(final String propertyName) {
        return Integer.parseInt(properties.getProperty(propertyName));
    }
}
