import com.google.gson.reflect.TypeToken;
import com.google.gson.stream.JsonReader;
import org.dinbur.ml.rl.monopoly.conf.Configuration;
import org.dinbur.ml.rl.monopoly.game.properties.MonopolyProperty;

import java.io.FileNotFoundException;
import java.io.FileReader;
import java.lang.reflect.Type;
import java.util.List;

import static org.dinbur.ml.rl.monopoly.game.properties.MonopolyProperty.GSON;

public class Main {

    private static final Type PROPERTY_TYPE = new TypeToken<List<MonopolyProperty>>() { }.getType();

    public static void main(String[] args) throws FileNotFoundException {
        JsonReader reader = new JsonReader(new FileReader(Configuration.getInstance().getStringValue("game-properties-file")));
        List<MonopolyProperty> data = GSON.fromJson(reader, PROPERTY_TYPE);
        System.out.println(data);
    }
}
